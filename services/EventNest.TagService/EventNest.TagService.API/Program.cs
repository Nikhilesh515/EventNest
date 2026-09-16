using EventNest.TagService.API;
using EventNest.TagService.API.Middleware;
using EventNest.TagService.API.Services;
using EventNest.TagService.Application;
using EventNest.TagService.Infrastructure;
using EventNest.Shared.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

JwtConfigurationGuard.Validate(
    builder.Configuration["Jwt:SecretKey"],
    builder.Environment.IsDevelopment());

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenAnyIP(51003, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGrpcReflectionService();
}

await app.MigrateAndSeed();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapGrpcService<TagGrpcService>();

app.Run();
