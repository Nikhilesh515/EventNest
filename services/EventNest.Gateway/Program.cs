using System.Text;
using EventNest.Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtSecretKey) || jwtSecretKey.Length < 32 ||
    (!builder.Environment.IsDevelopment() && jwtSecretKey == "YOUR_SECRET_KEY_HERE_MIN_32_CHARS_LONG!!"))
{
    throw new InvalidOperationException(
        "Jwt:SecretKey must be a real secret of at least 32 characters; the placeholder is rejected outside Development.");
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

var authHealthUrl = builder.Configuration["HealthChecks:AuthService"] ?? "http://localhost:5001/health";
var eventHealthUrl = builder.Configuration["HealthChecks:EventService"] ?? "http://localhost:5002/health";
var tagHealthUrl = builder.Configuration["HealthChecks:TagService"] ?? "http://localhost:5003/health";
var rsvpHealthUrl = builder.Configuration["HealthChecks:RSVPService"] ?? "http://localhost:5004/health";

builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
    .AddUrlGroup(new Uri(authHealthUrl), name: "auth-service")
    .AddUrlGroup(new Uri(eventHealthUrl), name: "event-service")
    .AddUrlGroup(new Uri(tagHealthUrl), name: "tag-service")
    .AddUrlGroup(new Uri(rsvpHealthUrl), name: "rsvp-service");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173"];

builder.Services.Configure<CorsOptions>(opts => opts.AllowedOrigins = allowedOrigins);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-Request-Id");
    });
});

var app = builder.Build();

app.UseMiddleware<GatewayExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

app.UseMiddleware<CorsMiddleware>();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapReverseProxy();

app.Run();
