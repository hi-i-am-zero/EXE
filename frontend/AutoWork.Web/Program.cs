var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", (IConfiguration config) => Results.Json(new
{
    apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7165"
}));

app.MapGet("/app", () => Results.Redirect("/app/dashboard.html"));

app.Run();
