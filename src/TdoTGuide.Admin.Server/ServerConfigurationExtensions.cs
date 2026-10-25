using Microsoft.Identity.Web;
using System.Reflection;
using TdoTGuide.Server.Common;

public static class ServerConfigurationExtensions
{
    public static IServiceCollection AddTdoTGuideAuthorizationRules(this IServiceCollection services)
    {
        return services.AddAuthorization(options =>
        {
            // By default, all incoming requests will be authorized according to the default policy
            //options.FallbackPolicy = options.DefaultPolicy;

            options.AddPolicy("ListProjects", policy => policy.RequireRole("project-reader"));
            options.AddPolicy("CreateProject", policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (ctx.User.IsInRole("all-projects-editor"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("project-creator"))
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
                    if (ctx.User.IsInRole("all-projects-editor"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("project-creator") && ctx.Resource is Project p && p.Organizer.Id == ctx.User.GetObjectId())
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
                    if (ctx.User.IsInRole("all-projects-editor"))
                    {
                        return true;
                    }
                    if (ctx.User.IsInRole("project-creator") && ctx.Resource is Project p && p.Organizer.Id == ctx.User.GetObjectId())
                    {
                        return true;
                    }
                    return false;
                });
            });

            options.AddPolicy("ListDisabledProject", policy =>
            {
                policy.RequireAssertion(ctx =>
                {
                    if (ctx.User.IsInRole("all-projects-editor"))
                    {
                        return true;
                    }
                    if (ctx.Resource is Project p && (p.Organizer.Id == ctx.User.GetObjectId() || p.CoOrganizers.Any(v => v.Id == ctx.User.GetObjectId())))
                    {
                        return true;
                    }
                    return false;
                });
            });

            options.AddPolicy("ChangeProjectOrganizer", policy => policy.RequireRole("all-projects-editor"));
        });
    }

    public static IMvcBuilder AddTdoTGuideControllers(this IServiceCollection services)
    {
        return services
            .AddControllersWithViews()
            .AddApplicationPart(Assembly.GetExecutingAssembly()); // for tests
    }
}
