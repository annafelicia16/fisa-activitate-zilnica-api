using System;
using System.IO;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.Data.SqlClient;
using FisaActivitateZilnicaApi.DailyActivities.Repositories;
using FisaActivitateZilnicaApi.DailyActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.DailyActivities.Services;
using FisaActivitateZilnicaApi.DailyActivities.Services.Interfaces;
using FisaActivitateZilnicaApi.Data.Master;
using FisaActivitateZilnicaApi.Data.University;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories;
using FisaActivitateZilnicaApi.ExternalReferences.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.Repositories;
using FisaActivitateZilnicaApi.ExternalTeachers.Repositories.Interfaces;
using FisaActivitateZilnicaApi.ExternalTeachers.Services;
using FisaActivitateZilnicaApi.ExternalTeachers.Services.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Repositories;
using FisaActivitateZilnicaApi.Schedules.Repositories.Interfaces;
using FisaActivitateZilnicaApi.Schedules.Services;
using FisaActivitateZilnicaApi.Schedules.Services.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories;
using FisaActivitateZilnicaApi.SupplementaryActivities.Repositories.Interfaces;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services;
using FisaActivitateZilnicaApi.SupplementaryActivities.Services.Interfaces;
using FisaActivitateZilnicaApi.System;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;

QuestPDF.Settings.License = LicenseType.Community;

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
    "CLIENT_URL",
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
builder.Services.AddScoped<IDailyActivityRecordsRepository, DailyActivityRecordsRepository>();
builder.Services.AddScoped<IExternalTeachersRepository, ExternalTeachersRepository>();
builder.Services.AddScoped<IExternalReferencesRepository, ExternalReferencesRepository>();
builder.Services.AddScoped<
    ISupplementaryActivitiesRepository,
    SupplementaryActivitiesRepository
>();

#endregion

#region Services

builder.Services.AddScoped<IFetImportService, FetImportService>();
builder.Services.AddScoped<ISchedulesQueryService, SchedulesQueryService>();
builder.Services.AddScoped<IDailyActivityRecordsQueryService, DailyActivityRecordsQueryService>();
builder.Services.AddScoped<IExternalTeachersQueryService, ExternalTeachersQueryService>();
builder.Services.AddScoped<
    IDailyActivityRecordsCommandService,
    DailyActivityRecordsCommandService
>();
builder.Services.AddScoped<
    ISupplementaryActivitiesQueryService,
    SupplementaryActivitiesQueryService
>();
builder.Services.AddScoped<
    ISupplementaryActivitiesCommandService,
    SupplementaryActivitiesCommandService
>();

#endregion

#region Helpers

#endregion

#region Utility

#endregion

#region Configuration

builder.Services.AddLogging();
builder.Services.AddHttpClient();

// Add CORS for the configured client URL
string clientUrl = Env.GetString("CLIENT_URL");
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Client",
        policy =>
        {
            policy
                .WithOrigins(clientUrl)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
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
    + "MultipleActiveResultSets=False;"
    + "Command Timeout=0";

builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseSqlServer(universityConnectionString)
);

string masterConnectionString =
    $"Server={Env.GetString("MASTER_DB_HOST")},{Env.GetString("MASTER_DB_PORT")};"
    + $"Database={Env.GetString("MASTER_DB_DATABASE")};"
    + $"User Id={Env.GetString("MASTER_DB_USERNAME")};"
    + $"Password={Env.GetString("MASTER_DB_PASSWORD")};"
    + "Encrypt=True;"
    + "TrustServerCertificate=True;";

builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(masterConnectionString)
);

#endregion

#region Fluent Migrator

builder
    .Services.AddFluentMigratorCore()
    .ConfigureRunner(rb =>
        rb.AddSqlServer()
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
app.UseCors("Client");

app.MapControllers();

await WaitForDatabaseAsync(masterConnectionString, TimeSpan.FromMinutes(2));
await EnsureDatabaseExistsAsync(masterConnectionString);

using (IServiceScope scope = app.Services.CreateScope())
{
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.Run();

static async Task EnsureDatabaseExistsAsync(string connectionString)
{
    var target = new SqlConnectionStringBuilder(connectionString);
    string targetDatabase = target.InitialCatalog;
    if (string.IsNullOrWhiteSpace(targetDatabase))
    {
        return;
    }

    var masterBuilder = new SqlConnectionStringBuilder(connectionString)
    {
        InitialCatalog = "master",
    };

    await using var connection = new SqlConnection(masterBuilder.ConnectionString);
    await connection.OpenAsync();

    await using var checkCommand = connection.CreateCommand();
    checkCommand.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
    checkCommand.Parameters.Add(new SqlParameter("@name", targetDatabase));
    object? exists = await checkCommand.ExecuteScalarAsync();
    if (exists != null)
    {
        return;
    }

    string escaped = targetDatabase.Replace("]", "]]");
    await using var createCommand = connection.CreateCommand();
    createCommand.CommandText = $"CREATE DATABASE [{escaped}]";
    await createCommand.ExecuteNonQueryAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✅ Created database '{targetDatabase}'.");
    Console.ResetColor();
}

static async Task WaitForDatabaseAsync(string connectionString, TimeSpan timeout)
{
    var builder = new SqlConnectionStringBuilder(connectionString)
    {
        ConnectTimeout = 3,
        InitialCatalog = "master",
    };
    string probeConnectionString = builder.ConnectionString;

    DateTime deadline = DateTime.UtcNow + timeout;
    int attempt = 0;
    Console.WriteLine("Waiting for SQL Server to accept connections...");

    while (true)
    {
        attempt++;
        try
        {
            await using var connection = new SqlConnection(probeConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ SQL Server is ready (attempt {attempt}).");
            Console.ResetColor();
            return;
        }
        catch (Exception ex)
        {
            if (DateTime.UtcNow >= deadline)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"❌ SQL Server did not become ready within {timeout.TotalSeconds:F0}s."
                );
                Console.ResetColor();
                throw;
            }
            Console.WriteLine(
                $"  attempt {attempt}: not ready yet ({ex.GetType().Name}: {ex.Message.Split('\n')[0]}). Retrying in 2s..."
            );
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
