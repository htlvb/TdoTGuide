using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Minio;
using System.Globalization;
using TdoTGuide.Admin.Server;
using TdoTGuide.Server.Common;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-AT");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        builder.Configuration.GetSection("Oidc").Bind(options);
    });
builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();
builder.Services.AddTdoTGuideControllers();
builder.Services.AddRazorPages();

builder.Services.AddTdoTGuideAuthorizationRules();

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(provider =>
{
    var baseUrl = builder.Configuration.GetSection("Keycloak")["BaseUrl"] ?? throw new Exception("Can't find Keycloak\\BaseUrl");
    var accessTokenProvider = new KeycloakAccessTokenProvider(
        new Uri(baseUrl).Host,
        provider.GetRequiredService<IHttpContextAccessor>()
    );
    return new KeycloakAdminApiClientFactory(baseUrl, accessTokenProvider);
});
builder.Services.AddScoped<IUserStore>(provider =>
{
    return new KeycloakUserStore(
        builder.Configuration.GetSection("Keycloak")["RealmName"] ?? throw new Exception("Can't find Keycloak\\RealmName"),
        builder.Configuration.GetSection("AppSettings")["OrganizerGroupId"] ?? throw new Exception("Can't find AppSettings\\OrganizerGroupId"),
        provider.GetRequiredService<KeycloakAdminApiClientFactory>());
});

var app = builder.Build();

app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedProto });

app.UseRequestLocalization(CultureInfo.CurrentCulture.Name);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true }); // support let's encrypt challenge

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
