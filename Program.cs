using System;
using System.IO;
using System.Text.Json.Serialization;
using DotNetEnv;
using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.Data.University;
using FisaActivitateZilnicaApi.Schedules.Repositories;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;
using FisaActivitateZilnicaApi.System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Console.WriteLine("Loading .env file...");
    Env.Load();
}
else
{
    Console.WriteLine(".env file not found. Using system environment variables.");
}

#region VALIDATE ENVIRONMENT VARIABLES

// Validate all required environment variables
string[] requiredEnvVars =
[
    "UNIVERSITY_DB_HOST",
    "UNIVERSITY_DB_PORT",
    "UNIVERSITY_DB_DATABASE",
    "UNIVERSITY_DB_USERNAME",
    "UNIVERSITY_DB_PASSWORD",
    "MASTER_DB_HOST",
    "MASTER_DB_PORT",
    "MASTER_DB_DATABASE",
    "MASTER_DB_USERNAME",
    "MASTER_DB_PASSWORD",
    "API_PORT",
    "ENVIRONMENT",
];

Console.WriteLine("Checking environment variables...\n");
int missingCount = 0;

foreach (var envVar in requiredEnvVars)
{
    var value = Env.GetString(envVar) ?? Environment.GetEnvironmentVariable(envVar);
    if (string.IsNullOrEmpty(value))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {envVar}: Missing or empty");
        missingCount++;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {envVar}: Present");
    }
    Console.ResetColor();
}

Console.WriteLine($"\nTotal missing variables: {missingCount}");

if (missingCount > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(
        "\nPlease ensure all required environment variables are set in your .env file."
    );
    Console.ResetColor();
    return;
}

#endregion

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddAutoMapper(typeof(MappingProfile));

#region Repositories

builder.Services.AddScoped<ISchedulesRepository, SchedulesRepository>();

#endregion

#region Services

builder.Services.AddScoped<IFetImportService, FetImportService>();
builder.Services.AddScoped<ISchedulesQueryService, SchedulesQueryService>();

#endregion

#region Helpers

#endregion

#region Utility

#endregion

#region Configuration

builder.Services.AddLogging();
builder.Services.AddHttpClient();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

// Add Swagger only in non-production environments
string environment = Env.GetString("ENVIRONMENT") ?? "development";
if (!string.Equals(environment, "production", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(int.Parse(Env.GetString("API_PORT")));
});

// Configure logging based on environment
if (string.Equals(environment, "production", StringComparison.OrdinalIgnoreCase))
{
    // In production, disable all logging providers
    builder.Logging.ClearProviders();
    // Optionally, you can set minimum log level to None to completely disable logging
    builder.Logging.SetMinimumLevel(LogLevel.None);
}
else
{
    // In non-production environments, use console logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

string universityConnectionString =
    $"Data Source={Env.GetString("UNIVERSITY_DB_HOST")};"
    + $"Initial Catalog={Env.GetString("UNIVERSITY_DB_DATABASE")};"
    + $"User ID={Env.GetString("UNIVERSITY_DB_USERNAME")};"
    + $"Password={Env.GetString("UNIVERSITY_DB_PASSWORD")};"
    + "Persist Security Info=True;"
    + "Encrypt=True;"
    + "TrustServerCertificate=True;"
    + "Pooling=False;"
    + "MultipleActiveResultSets=False;";

builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseSqlServer(universityConnectionString)
);

string masterConnectionString =
    $"Server={Env.GetString("MASTER_DB_HOST")};"
    + $"Port={Env.GetString("MASTER_DB_PORT")};"
    + $"Database={Env.GetString("MASTER_DB_DATABASE")};"
    + $"Uid={Env.GetString("MASTER_DB_USERNAME")};"
    + $"Pwd={Env.GetString("MASTER_DB_PASSWORD")};";

builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(masterConnectionString, ServerVersion.AutoDetect(masterConnectionString))
);

#endregion

#region Fluent Migrator

builder
    .Services.AddFluentMigratorCore()
    .ConfigureRunner(rb =>
        rb.AddMySql8()
            .WithGlobalConnectionString(masterConnectionString)
            .ScanIn(typeof(Program).Assembly)
            .For.Migrations()
    )
    .AddLogging(lb => lb.AddFluentMigratorConsole());

#endregion

WebApplication app = builder.Build();

// Configure Swagger only in non-production environments
if (!string.Equals(environment, "production", StringComparison.OrdinalIgnoreCase))
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Fisa Activitate Zilnica API")
            .WithTheme(ScalarTheme.Default)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Use CORS
app.UseCors("AllowAll");

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.Run();
