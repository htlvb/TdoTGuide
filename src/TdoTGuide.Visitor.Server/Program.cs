using Minio;
using System.Text.Json.Serialization;
using TdoTGuide.Server.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMinio(minioClient => minioClient
    .WithEndpoint(builder.Configuration.GetSection("Minio")["Endpoint"])
    .WithSSL(builder.Configuration.GetSection("Minio").GetValue("UseSSL", true))
    .WithCredentials(
        builder.Configuration.GetSection("Minio")["AccessKey"],
        builder.Configuration.GetSection("Minio")["SecretKey"]
    )
    .Build()
);

builder.Services.AddSingleton(new PgsqlConnectionString(builder.Configuration.GetConnectionString("Pgsql") ?? throw new Exception("Can't find ConnectionStrings\\Pgsql.")));
builder.Services.AddSingleton<IProjectStore, PgsqlProjectStore>();
builder.Services.AddSingleton<IBuildingStore, PgsqlBuildingStore>();
builder.Services.AddSingleton<IProjectMediaStore, MinioProjectMediaStore>();

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
