using System.Text;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Infrastructure.AI;
using AutoWork.Infrastructure.Integrations;
using AutoWork.Infrastructure.Jobs;
using AutoWork.Infrastructure.Payments;
using AutoWork.Infrastructure.Services;
using AutoWork.Infrastructure.Settings;
using AutoWork.Infrastructure.SignalR;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AutoWork.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AiSettings>(configuration.GetSection(AiSettings.SectionName));
        services.Configure<FacebookSettings>(configuration.GetSection(FacebookSettings.SectionName));
        services.Configure<WordPressSettings>(configuration.GetSection(WordPressSettings.SectionName));
        services.Configure<ZaloSettings>(configuration.GetSection(ZaloSettings.SectionName));
        services.Configure<PaymentSettings>(configuration.GetSection(PaymentSettings.SectionName));
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings is required.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments(NotificationHubClient.HubPath))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSingleton<IntegrationTokenStore>();

        services.AddHttpClient<OpenAiProvider>();
        services.AddHttpClient<GeminiAiProvider>();
        services.AddHttpClient<ClaudeAiProvider>();
        services.AddHttpClient<MoMoService>();
        services.AddHttpClient<ZaloPayService>();
        services.AddHttpClient<FacebookService>();
        services.AddHttpClient<WordPressService>();
        services.AddHttpClient<ZaloService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICreditService, CreditService>();
        services.AddScoped<IAiContentService, AiContentService>();
        services.AddScoped<IFacebookService, FacebookService>();
        services.AddScoped<IWordPressService, WordPressService>();
        services.AddScoped<IZaloService, ZaloService>();
        services.AddScoped<IPaymentService, PaymentService>();

        var storageProvider = configuration.GetSection(StorageSettings.SectionName).Get<StorageSettings>()?.Provider ?? "Local";
        if (string.Equals(storageProvider, "Azure", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IStorageService, LocalStorageService>();
        }

        services.AddScoped<IAiProvider, OpenAiProvider>();
        services.AddScoped<IAiProvider, GeminiAiProvider>();
        services.AddScoped<IAiProvider, ClaudeAiProvider>();
        services.AddScoped<AiProviderFactory>();
        services.AddScoped<VNPayService>();
        services.AddScoped<MoMoService>();
        services.AddScoped<ZaloPayService>();
        services.AddScoped<PostScheduleJob>();
        services.AddScoped<PublishPostJob>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is required.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(15)
            }));
        services.AddHangfireServer();
        services.AddSignalR();

        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.MapHub<NotificationHub>(NotificationHubClient.HubPath);
        HangfireJobRegistration.RegisterRecurringJobs();
        return app;
    }
}
