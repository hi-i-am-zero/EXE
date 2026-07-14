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

        var hasExistingSchema = await TableExistsAsync(context, "Users", cancellationToken);
        var hasMigrationHistory = await TableExistsAsync(context, "__EFMigrationsHistory", cancellationToken);

        if (hasExistingSchema && !hasMigrationHistory)
        {
            logger?.LogInformation("Detected FlowMate/SQL schema without EF history — applying compat layer and skipping MigrateAsync.");
            await ApplyFlowMateCompatAsync(context, logger, cancellationToken);
        }
        else
        {
            await context.Database.MigrateAsync(cancellationToken);
        }

        await DataSeeder.SeedAsync(context, logger, cancellationToken);
    }

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext context,
        string tableName,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @table
                ) THEN 1 ELSE 0 END
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@table";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result) == 1;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static async Task ApplyFlowMateCompatAsync(
        ApplicationDbContext context,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var statements = new[]
        {
            """
            IF COL_LENGTH('dbo.Posts', 'ChannelAccountId') IS NULL
                ALTER TABLE dbo.Posts ADD ChannelAccountId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'ExternalPostId') IS NULL
                ALTER TABLE dbo.Posts ADD ExternalPostId NVARCHAR(128) NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'PublishedUrl') IS NULL
                ALTER TABLE dbo.Posts ADD PublishedUrl NVARCHAR(1000) NULL;
            """,
            """
            IF COL_LENGTH('dbo.Posts', 'PublishedAt') IS NULL
                ALTER TABLE dbo.Posts ADD PublishedAt DATETIME2 NULL;
            """,
            """
            IF COL_LENGTH('dbo.PostSchedules', 'PostId') IS NULL
                ALTER TABLE dbo.PostSchedules ADD PostId UNIQUEIDENTIFIER NULL;
            """,
            """
            IF COL_LENGTH('dbo.PostSchedules', 'PostChannelAccountId') IS NOT NULL
               AND COLUMNPROPERTY(OBJECT_ID('dbo.PostSchedules'), 'PostChannelAccountId', 'AllowsNull') = 0
            BEGIN
                ALTER TABLE dbo.PostSchedules ALTER COLUMN PostChannelAccountId UNIQUEIDENTIFIER NULL;
            END
            """
        };

        foreach (var sql in statements)
        {
            await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        logger?.LogInformation("FlowMate EF compatibility layer applied.");
    }
}
