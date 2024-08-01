using Minio;
using Minio.DataModel.Args;
using System.Text.RegularExpressions;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Server.Common;

public class MinioProjectMediaStore(IMinioClient minioClient) : IProjectMediaStore
{
    private static string BucketName => "project-media";

    public async IAsyncEnumerable<string> GetNewMediaUploadUrls(string projectId, IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            yield return await minioClient.PresignedPutObjectAsync(new PresignedPutObjectArgs()
                .WithBucket(BucketName)
                .WithObject($"{projectId}/{fileName}")
                .WithExpiry((int)TimeSpan.FromMinutes(1).TotalSeconds));
        }
    }

    public async Task RemoveMedia(string projectId, IEnumerable<string> fileNames)
    {
        var objects = fileNames.Select(v => $"{projectId}/{v}").ToList();
        if (objects.Count == 0)
        {
            return;
        }

        await EnsureBucketExists();
        await minioClient.RemoveObjectsAsync(new RemoveObjectsArgs()
            .WithBucket(BucketName)
            .WithObjects(objects));
    }

    public async IAsyncEnumerable<string> GetAllMediaNames(string projectId)
    {
        await EnsureBucketExists();
        var dirName = $"{projectId}/";
        var files = minioClient.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(BucketName).WithPrefix(dirName));
        await foreach (var file in files)
        {
            yield return file.Key[dirName.Length..];
        }
    }

    public async Task<Dictionary<string, List<ProjectMedia>>> GetAllMedia(IEnumerable<string> projectIds)
    {
        await EnsureBucketExists();
        Dictionary<string, List<ProjectMedia>> projectMedia = [];
        var projectDirs = minioClient.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(BucketName));
        await foreach (var projectDir in projectDirs)
        {
            if (!projectDir.IsDir) continue;

            List<ProjectMedia> media = [];
            var files = minioClient.ListObjectsEnumAsync(new ListObjectsArgs()
                .WithBucket(BucketName)
                .WithPrefix(projectDir.Key)
                .WithIncludeUserMetadata(true));
            await foreach (var file in files)
            {
                ProjectMediaType type;
                if (Regex.IsMatch(file.ContentType, "^(image/jpeg|image/png)$", RegexOptions.IgnoreCase))
                {
                    type = ProjectMediaType.Image;
                }
                else if (Regex.IsMatch(file.ContentType, "^(video/mp4)$", RegexOptions.IgnoreCase))
                {
                    type = ProjectMediaType.Video;
                }
                else
                {
                    continue;
                }
                var fileUrl = await minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(file.Key)
                    .WithExpiry((int)TimeSpan.FromDays(1).TotalSeconds));
                media.Add(new ProjectMedia(type, fileUrl));
            }
            projectMedia.Add(projectDir.Key.TrimEnd('/'), media);
        }
        return projectMedia;
    }

    private async Task EnsureBucketExists()
    {
        await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
    }
}