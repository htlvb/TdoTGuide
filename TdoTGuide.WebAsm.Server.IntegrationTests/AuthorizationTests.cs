﻿using FsCheck.Fluent;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TdoTGuide.WebAsm.Server.Data;
using TdoTGuide.WebAsm.Server.IntegrationTests.Utils;
using System.Net;
using System.Net.Http.Json;

namespace TdoTGuide.WebAsm.Server.IntegrationTests;

public class AuthorizationTests
{
    public static IEnumerable<object?[]> ResourcesWithAuthorization()
    {
        yield return new object?[]
        {
            Enumerable.Empty<Project>(),
            HttpMethod.Get,
            "/api/projects",
            default(HttpContent?)
        };
        yield return new object?[]
        {
            Enumerable.Empty<Project>(),
            HttpMethod.Get,
            $"/api/projects/edit/new",
            default(HttpContent?)

        };
    }

    [Theory]
    [MemberData(nameof(ResourcesWithAuthorization))]
    public async Task CantAccessProjectResourcesWhenNotAuthenticated(
        IEnumerable<Project> projects,
        HttpMethod httpMethod,
        string url,
        HttpContent httpContent)
    {
        // Arrange
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        foreach (var project in projects)
        {
            await projectStore.Create(project);
        }
        using var client = host.GetTestClient();
        using var request = new HttpRequestMessage(httpMethod, url)
        {
            Content = httpContent
        };

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
