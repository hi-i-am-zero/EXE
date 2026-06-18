using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Persistence.Context;
using AutoWork.Persistence.Repositories;
using AutoWork.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoWork.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(3);
            }));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ICreditRepository, CreditRepository>();
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IFacebookRepository, FacebookRepository>();
        services.AddScoped<IWordPressRepository, WordPressRepository>();
        services.AddScoped<IZaloRepository, ZaloRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IAffiliateRepository, AffiliateRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync(cancellationToken);
        await DataSeeder.SeedAsync(context, logger, cancellationToken);
    }
}
