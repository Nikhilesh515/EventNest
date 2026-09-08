using EventNest.AuthService.API;
using EventNest.AuthService.Application;
using EventNest.AuthService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

await app.MigrateAndSeed();
app.UseApiMiddleware();

app.Run();
