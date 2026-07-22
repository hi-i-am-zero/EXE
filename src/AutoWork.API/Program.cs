using AspNetCoreRateLimit;
using AutoWork.API.Authorization;
using AutoWork.API.Configuration;
using AutoWork.API.Filters;
using AutoWork.API.Middleware;
using AutoWork.Application;
using AutoWork.Infrastructure;
using AutoWork.Persistence;
using AutoWork.Shared.Constants;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEmailPropertiesFile("email.properties");
builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/autowork-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options => options.Filters.Add<ValidateModelAttribute>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoWork API",
        Version = "v1",
        Description = "SaaS Marketing Automation Platform API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in AppPermissions.All)
    {
        options.AddPolicy($"Permission:{permission}", policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

LogEmailConfiguration(app.Configuration, app.Environment.IsDevelopment());

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoWork API v1"));
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Default");
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] });
app.UseInfrastructure();

app.Run();

static void LogEmailConfiguration(IConfiguration configuration, bool isDevelopment)
{
    var provider = configuration["EmailSettings:Provider"] ?? "Auto";
    var brevoKey = configuration["EmailSettings:BrevoApiKey"];
    var host = configuration["EmailSettings:SmtpHost"];
    var user = configuration["EmailSettings:Username"];
    var pass = configuration["EmailSettings:Password"];
    var from = configuration["EmailSettings:FromEmail"];
    var devFallback = configuration["EmailSettings:UseDevFileFallback"];

    if (!string.IsNullOrWhiteSpace(brevoKey) && !string.IsNullOrWhiteSpace(from ?? user))
    {
        Log.Information("Email Brevo configured — gửi được tới mọi địa chỉ người dùng (From: {From})", from ?? user);
        return;
    }

    if (!string.IsNullOrWhiteSpace(host)
        && !string.IsNullOrWhiteSpace(user)
        && !string.IsNullOrWhiteSpace(pass)
        && (!string.IsNullOrWhiteSpace(from) || !string.IsNullOrWhiteSpace(user)))
    {
        Log.Information("Email SMTP configured: {Host} as {User} — gửi tới bất kỳ email người dùng nào", host, user);
        return;
    }

    if (isDevelopment && !string.Equals(devFallback, "false", StringComparison.OrdinalIgnoreCase))
    {
        Log.Warning(
            "Email chưa cấu hình Brevo/SMTP — dùng DevFile: lưu email vào logs/emails/ (mọi địa chỉ người dùng). " +
            "Để gửi mail thật: cập nhật src/AutoWork.API/email.properties hoặc chạy scripts/configure-email.ps1");
        return;
    }

    Log.Warning(
        "Email chưa cấu hình — không gửi được mail xác nhận/OTP. " +
        "Cập nhật email.properties hoặc appsettings.Development.local.json");
}

public partial class Program;
