using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("ApiProxy", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

var app = builder.Build();

var apiBaseUrl = (builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7165").TrimEnd('/');
var useProxy = builder.Configuration.GetValue("ApiSettings:UseProxy", app.Environment.IsDevelopment());

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/config", () => Results.Json(new
{
    apiBaseUrl = useProxy ? "" : apiBaseUrl,
    useProxy
}));

app.MapGet("/app", () => Results.Redirect("/app/dashboard.html"));

if (useProxy)
{
    app.Map("/backend-api/{**path}", async (HttpContext ctx, IHttpClientFactory factory) =>
    {
        var path = ctx.Request.Path.Value!.Replace("/backend-api", "", StringComparison.OrdinalIgnoreCase);
        var target = $"{apiBaseUrl}{path}{ctx.Request.QueryString}";

        using var request = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), target);

        if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Content-Type"))
        {
            request.Content = new StreamContent(ctx.Request.Body);
            if (!string.IsNullOrEmpty(ctx.Request.ContentType))
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
        }

        foreach (var header in ctx.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;
            if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)) continue;
            request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        var client = factory.CreateClient("ApiProxy");
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);

        ctx.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
            ctx.Response.Headers[header.Key] = header.Value.ToArray();
        foreach (var header in response.Content.Headers)
            ctx.Response.Headers[header.Key] = header.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");

        await response.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
    });
}

app.Run();
