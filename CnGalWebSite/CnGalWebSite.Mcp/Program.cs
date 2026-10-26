using System.Security.Cryptography;
using System.Text;
using CnGalWebSite.Mcp;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Queries;

var builder = WebApplication.CreateBuilder(args);
var token = builder.Configuration["Mcp:Token"];
if (string.IsNullOrWhiteSpace(token) || token.Length < 32)
{
    throw new InvalidOperationException("Set Mcp__Token to a dedicated token of at least 32 characters.");
}
var api = new Uri(builder.Configuration["CnGal:ApiBaseUrl"] ?? "https://api.cngal.org/");
if (api.Scheme is not ("http" or "https")) throw new InvalidOperationException("Invalid API base URL.");
builder.Services.AddHttpClient<IRobotQueryService, RobotQueryService>(client =>
{
    client.BaseAddress = new Uri(api.AbsoluteUri.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddMcpServer().WithHttpTransport(options => options.Stateless = true).WithTools<RobotTools>();
var app = builder.Build();
var expected = SHA256.HashData(Encoding.UTF8.GetBytes(token));
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/mcp"))
    {
        var header = context.Request.Headers.Authorization.ToString();
        var actual = header.StartsWith("Bearer ", StringComparison.Ordinal) ? header[7..] : "";
        if (!CryptographicOperations.FixedTimeEquals(expected, SHA256.HashData(Encoding.UTF8.GetBytes(actual))))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
    }
    await next(context);
});
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapMcp("/mcp");
app.Run();

public partial class Program;
