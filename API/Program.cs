using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using API.Middleware;
using Microsoft.AspNetCore.Mvc;
using API.Errors;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to include console output
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
   
// ADD RETRY LOGIC HERE
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            // Enable retry on failure for transient errors
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            
            // Set command timeout to 60 seconds
            sqlOptions.CommandTimeout(60);
        }));

// ADD REDIS RETRY LOGIC
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Redis") 
        ?? throw new InvalidOperationException("Redis connection string not found");
    
    var configuration = ConfigurationOptions.Parse(connString, true);
    configuration.ConnectRetry = 5;
    configuration.ConnectTimeout = 10000;
    configuration.SyncTimeout = 5000;
    configuration.AbortOnConnectFail = false;
    configuration.ReconnectRetryPolicy = new ExponentialRetry(5000);
    
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICartService, CartService>();

// FIX CORS - Remove duplicates and use single policy
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (isDevelopment)
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("https://shopnet2k6.azurewebsites.net")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToArray();

        var errorResponse = new ApiValidationErrorResponse(errors);

        return new BadRequestObjectResult(errorResponse);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up...");

await InitializeDatabaseAsync(app, logger);

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("CorsPolicy");

app.UseStatusCodePagesWithReExecute("/errors/{0}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNet API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

logger.LogInformation("Application started successfully");

app.Run();

async Task InitializeDatabaseAsync(WebApplication app, ILogger logger)
{
    const int maxRetries = 5;
    const int delayMilliseconds = 2000;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<StoreContext>();
            var scopedLogger = services.GetRequiredService<ILogger<Program>>();

            scopedLogger.LogInformation("Database initialization attempt {Attempt} of {MaxRetries}", attempt, maxRetries);

            // Test connection first
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new Exception("Cannot connect to database");
            }

            scopedLogger.LogInformation("Database connection successful");

            // Run migrations
            scopedLogger.LogInformation("Starting database migration...");
            await context.Database.MigrateAsync();
            scopedLogger.LogInformation("Database migration completed successfully");

            // Seed data
            scopedLogger.LogInformation("Starting database seeding...");
            await StoreContextSeed.SeedAsync(context, scopedLogger);
            scopedLogger.LogInformation("Database seeding completed successfully");

            return; // Success - exit retry loop
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization attempt {Attempt} failed: {Message}", attempt, ex.Message);

            if (attempt == maxRetries)
            {
                logger.LogCritical("Database initialization failed after {MaxRetries} attempts. Application will continue but database may not be initialized.", maxRetries);
                return;
            }

            var delay = delayMilliseconds * (int)Math.Pow(2, attempt - 1);
            logger.LogWarning("Retrying database initialization in {Delay}ms...", delay);
            await Task.Delay(delay);
        }
    }
}
