using System.Reflection;
using AutoWork.Application.Common.Behaviors;
using AutoWork.Application.Common.Mappings;
using AutoWork.Application.Common.Services;
using AutoWork.Application.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoWork.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var licenseKey = configuration["AutoMapper:LicenseKey"] ?? string.Empty;

        services.AddAutoMapper(cfg => cfg.LicenseKey = licenseKey, typeof(MappingProfile));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly);

        // FlowMate v2: dùng chung cho mọi feature thao tác trong phạm vi workspace
        services.AddScoped<IProjectAccessGuard, ProjectAccessGuard>();

        return services;
    }
}
