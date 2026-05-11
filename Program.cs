using MongoDB.Driver;
using NLog;
using NLog.Web;
using SessionService.Repository;
using SessionService.Repository.Interfaces;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

// Endpoint til vault, vault og Service skal være på samme docker netværk, så 'localhost' bliver til 'vault' i endpoint
var EndPoint = Environment.GetEnvironmentVariable("VAULT_URL") ?? "https://localhost:8201/";
var httpClientHandler = new HttpClientHandler();
httpClientHandler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) => { return true; };

// Initialize one of the several auth methods.
IAuthMethodInfo authMethod =
    new TokenAuthMethodInfo("00000000-0000-0000-0000-000000000000");

// Initialize settings
var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
{
    Namespace = "",
    MyHttpClientProviderFunc = handler
        => new HttpClient(httpClientHandler) {
            BaseAddress = new Uri(EndPoint)
        }
};

var logger = LogManager.Setup().LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("Starting SessionService");
logger.Debug("Connecting to Hashicorp Vault on: {0}", EndPoint);

IVaultClient vaultClient = new VaultClient(vaultClientSettings);

try
{
    logger.Debug("Getting MongoDB connection string and database name from Vault");
    Secret<SecretData> mongoSecrets = await vaultClient.V1.Secrets.KeyValue.V2
        .ReadSecretAsync(path: "mongo", mountPoint: "secret");
    
    string connectionString = mongoSecrets.Data.Data["MONGO_CONNECTION_STRING"]?.ToString() ?? throw new NullReferenceException("MONGO_CONNECTION_STRING not found in Vault");
    logger.Debug("MongoDB connection string loaded from Vault");
    Environment.SetEnvironmentVariable("MONGO_CONNECTION_STRING", connectionString);
    
    string mongoDbName = mongoSecrets.Data.Data["MONGO_SESSION_DB"]?.ToString() ?? throw new NullReferenceException("MONGO_SESSION_DB not found in Vault");
    logger.Debug("MongoDB database name loaded from Vault");
    Environment.SetEnvironmentVariable("MONGO_DATABASE_NAME", mongoDbName);
}
catch (Exception e)
{
    logger.Error($"Something went wrong connecting to Vault: {e.InnerException?.Message}");
    throw;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Clear default providers and use NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// HttpClient for UserService
builder.Services.AddHttpClient("userService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});

// MongoDB Setup
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("MONGO_CONNECTION_STRING environment variable is not set");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
    
    if (string.IsNullOrWhiteSpace(databaseName))
        throw new InvalidOperationException("MONGO_DATABASE_NAME environment variable is not set");
    
    return mongoClient.GetDatabase(databaseName);
});

// Register repositories
builder.Services.AddScoped<ICenterRepository, CenterRepositoryMongoDb>();
builder.Services.AddScoped<ISessionRepository, SessionRepositoryMongoDb>();
builder.Services.AddScoped<IBookingRepository, BookingRepositoryMongoDb>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
