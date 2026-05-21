using Microsoft.EntityFrameworkCore;
using ProjectDataService.Data;
using ProjectDataService.Data.Repositories;
using ProjectDataService.Data.Repositories.Implementations;
using ProjectDataService.Services;
using ProjectDataService.Services.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionInterceptor>();
});

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");

builder.Services.AddDbContext<GazellaDbContext>(options =>
    options.UseNpgsql(connectionString
        ?? throw new InvalidOperationException("Connection string 'PostgreSQL' is missing")));

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GazellaDbContext>();
    db.Database.EnsureCreated();
}

app.MapGrpcService<ProjectQueryService>();
app.MapGrpcService<ProjectManagementService>();
app.MapGrpcService<RegistrationService>();
app.MapGet("/", () => "Project Data Service - gRPC only");

await app.RunAsync();