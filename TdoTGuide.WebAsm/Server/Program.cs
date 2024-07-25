using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using TdoTGuide.WebAsm.Server.Data;
using System.Globalization;
using Minio;

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-AT");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(opts =>
    {
        var section = builder.Configuration.GetSection("GraphBeta");
        var scopes = section.GetSection("Scopes");
        if (scopes != null)
        {
            // overwrite scopes instead of concatenating
            opts.Scopes = [];
        }
        section.Bind(opts);
    })
    .AddInMemoryTokenCaches();
builder.Services.AddTdoTGuideControllers();
builder.Services.AddRazorPages();

builder.Services.AddTdoTGuideAuthorizationRules();

builder.Services.AddMinio(minioClient => minioClient
    .WithEndpoint(builder.Configuration.GetSection("Minio")["Endpoint"])
    .WithSSL(false)
    .WithCredentials(
        builder.Configuration.GetSection("Minio")["AccessKey"],
        builder.Configuration.GetSection("Minio")["SecretKey"]
    )
    .Build()
);

builder.Services.AddSingleton<IProjectStore>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("Pgsql") ?? throw new Exception("Can't find ConnectionStrings\\Pgsql.");
    return new PgsqlProjectStore(connectionString);
});

builder.Services.AddSingleton<IProjectMediaStore, MinioProjectMediaStore>();

builder.Services.AddScoped<IUserStore>(provider =>
{
    return new UserStore(
        builder.Configuration.GetSection("AppSettings")["OrganizerGroupId"] ?? throw new Exception("Can't find AppSettings\\OrganizerGroupId"),
        provider.GetRequiredService<GraphServiceClient>());
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
