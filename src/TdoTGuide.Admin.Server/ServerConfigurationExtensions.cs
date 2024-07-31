using Microsoft.Identity.Web;
using TdoTGuide.Admin.Server.Data;
using TdoTGuide.Admin.Shared;
using System.Reflection;

public static class ServerConfigurationExtensions
{
    public static IServiceCollection AddTdoTGuideAuthorizationRules(this IServiceCollection services)
    {
        return services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy
            //options.FallbackPolicy = options.DefaultPolicy;

            options.AddPolicy("ListProjects", policy => policy.RequireRole("Project.Read"));
            options.AddPolicy("CreateProject", policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (ctx.User.IsInRole("Project.Write.All"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("Project.Write"))
                    {
                        if (ctx.Resource == null)
                        {
                            return true;
                        }
                        if (ctx.Resource is Project p && p.Organizer.Id == ctx.User.GetObjectId())
                        {
                            return true;
                        }
                    }
                    return false;
                });
            });
            options.AddPolicy("UpdateProject", policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (ctx.User.IsInRole("Project.Write.All"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("Project.Write") && ctx.Resource is Project p && p.Organizer.Id == ctx.User.GetObjectId())
                    {
                        return true;
                    }
                    return false;
                });
            });

            options.AddPolicy("DeleteProject", policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (ctx.User.IsInRole("Project.Write.All"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("Project.Write") && ctx.Resource is Project p && p.Organizer.Id == ctx.User.GetObjectId())
                    {
                        return true;
                    }
                    return false;
                });
            });

            options.AddPolicy("ChangeProjectOrganizer", policy => policy.RequireRole("Project.Write.All"));
        });
    }

    public static IMvcBuilder AddTdoTGuideControllers(this IServiceCollection services)
    {
        return services
            .AddControllersWithViews()
            .AddApplicationPart(Assembly.GetExecutingAssembly()); // for tests
    }
}
