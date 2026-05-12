using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using NLog;
using NLog.Web;
using SessionService.Repository;
using SessionService.Repository.Interfaces;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

// Endpoint til vault, vault og Service skal være på samme docker netværk,
// så 'localhost' bliver til 'vault' i endpoint
var EndPoint = Environment.GetEnvironmentVariable("VAULT_URL")
               ?? "https://localhost:8201/";

var httpClientHandler = new HttpClientHandler();

httpClientHandler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) =>
    {
        return true; // Ignorerer certifikat fejl
    };

// Initialize one of the several auth methods.
IAuthMethodInfo authMethod =
    new TokenAuthMethodInfo(
        "00000000-0000-0000-0000-000000000000"); // Bruger token authentication til vault

// Initialize settings
var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
{
    Namespace = "",

    MyHttpClientProviderFunc = handler =>
        new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri(EndPoint) // Sætter base url til vault endpoint
        }
};

// Starter logger
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("Starting SessionService"); // Logger at service starter
logger.Debug("Connecting to Hashicorp Vault on: {0}", EndPoint); // Logger vault endpoint

IVaultClient vaultClient = new VaultClient(vaultClientSettings); // Opretter vault client

try
{
    // Henter JWT secret fra Vault
    logger.Debug("Getting JWT secret from Vault");

    Secret<SecretData> jwtSecret = await vaultClient
        .V1.Secrets.KeyValue.V2
        .ReadSecretAsync(
            path: "auth",
            mountPoint: "secret");

    string jwtSecretString = jwtSecret
        .Data.Data["JWT_SECRET"]?.ToString()
        ?? throw new NullReferenceException(
            "JWT_SECRET not found in Vault");

    logger.Debug("JWT secret loaded from Vault");

    Environment.SetEnvironmentVariable(
        "JWT_SECRET",
        jwtSecretString); // Gemmer JWT secret som environment variable

    // Henter MongoDB secrets fra Vault
    logger.Debug("Getting MongoDB connection string and database name from Vault");

    Secret<SecretData> mongoSecrets = await vaultClient
        .V1.Secrets.KeyValue.V2
        .ReadSecretAsync(
            path: "mongo",
            mountPoint: "secret"); // Henter mongo secrets fra vault

    string connectionString = mongoSecrets
        .Data.Data["MONGO_CONNECTION_STRING"]?.ToString()
        ?? throw new NullReferenceException(
            "MONGO_CONNECTION_STRING not found in Vault");

    logger.Debug("MongoDB connection string loaded from Vault");

    Environment.SetEnvironmentVariable(
        "MONGO_CONNECTION_STRING",
        connectionString); // Gemmer connection string som environment variable

    string mongoDbName = mongoSecrets
        .Data.Data["MONGO_SESSION_DB"]?.ToString()
        ?? throw new NullReferenceException(
            "MONGO_SESSION_DB not found in Vault");

    logger.Debug("MongoDB database name loaded from Vault");

    Environment.SetEnvironmentVariable(
        "MONGO_DATABASE_NAME",
        mongoDbName); // Gemmer db navn som environment variable
}
catch (Exception e)
{
    logger.Error(
        $"Something went wrong connecting to Vault: {e.InnerException?.Message}");

    throw;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(); // Tilføjer openapi/swagger
builder.Services.AddControllers();

// Tilføjer authorization
builder.Services.AddAuthorization();

// Tilføjer JWT authentication
builder.Services.AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_SECRET")
                    ?? throw new InvalidOperationException(
                        "JWT_SECRET environment variable is not set"))),

            // Sat til false fordi microservices bruger samme interne JWT
            ValidateIssuer = false,
            ValidateAudience = false,

            ClockSkew = TimeSpan.Zero // Fjerner default delay på token expiration
        };
    });

// Clear default providers and use NLog
builder.Logging.ClearProviders(); // Fjerner default logging providers
builder.Host.UseNLog(); // Bruger NLog som logger

// HttpClient for UserService
builder.Services.AddHttpClient("userService", client =>
{
    client.BaseAddress = new Uri(
        "http://localhost:5000"); // Base url til userservice
});

// MongoDB Setup
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = Environment
        .GetEnvironmentVariable(
            "MONGO_CONNECTION_STRING"); // Henter mongo connection string

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "MONGO_CONNECTION_STRING environment variable is not set");
    }

    return new MongoClient(connectionString); // Opretter mongo client
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoClient =
        sp.GetRequiredService<IMongoClient>(); // Henter mongo client

    var databaseName = Environment
        .GetEnvironmentVariable(
            "MONGO_DATABASE_NAME"); // Henter db navn

    if (string.IsNullOrWhiteSpace(databaseName))
    {
        throw new InvalidOperationException(
            "MONGO_DATABASE_NAME environment variable is not set");
    }

    return mongoClient.GetDatabase(databaseName); // Returnerer mongo database
});

// Register repositories
builder.Services.AddScoped<ICenterRepository, CenterRepositoryMongoDb>();
builder.Services.AddScoped<ISessionRepository, SessionRepositoryMongoDb>();
builder.Services.AddScoped<IBookingRepository, BookingRepositoryMongoDb>();

var app = builder.Build(); // Builder application

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Aktiverer openapi/swagger
}

app.UseHttpsRedirection(); // Redirecter http requests til https

// Authentication SKAL komme før Authorization
app.UseAuthentication(); // Validerer JWT token
app.UseAuthorization(); // Tjekker brugerens adgang

app.MapControllers(); // Mapper controllers

app.Run(); // Starter application