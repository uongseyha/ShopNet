using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using API.Middleware;
using Microsoft.AspNetCore.Mvc;
using API.Errors;
using StackExchange.Redis;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();

// Configure DbContext with retry logic and connection pooling
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            
            sqlOptions.CommandTimeout(60);
        })
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// Configure Redis with lazy connection (doesn't block startup)
builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var connString = configuration.GetConnectionString("Redis") 
            ?? throw new InvalidOperationException("Redis connection string not found");
        
        var redisConfig = ConfigurationOptions.Parse(connString, true);
        redisConfig.ConnectRetry = 3;
        redisConfig.ConnectTimeout = 5000;
        redisConfig.SyncTimeout = 3000;
        redisConfig.AbortOnConnectFail = false;
        redisConfig.ReconnectRetryPolicy = new ExponentialRetry(3000);
        
        return ConnectionMultiplexer.Connect(redisConfig);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to Redis");
        throw;
    }
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<StoreContext>();

// Configure cookie policy for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.Name = ".AspNetCore.Identity.Application";
});

// Configure CORS - Fixed duplicate AllowCredentials
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

// ✅ CRITICAL: Only run migrations in Development or when explicitly enabled
var shouldMigrate = app.Configuration.GetValue<bool>("RunMigrations", false) || app.Environment.IsDevelopment();

if (shouldMigrate)
{
    logger.LogInformation("Migration mode enabled. Running database initialization...");
    await InitializeDatabaseAsync(app, logger);
}
else
{
    logger.LogInformation("Production mode: Skipping migrations. Database should already be initialized.");
}

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

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();

app.MapFallbackToController("Index", "Fallback");

logger.LogInformation("Application started successfully");

app.Run();

async Task InitializeDatabaseAsync(WebApplication app, ILogger logger)
{
    const int maxRetries = 3; // Reduced from 5
    const int delayMilliseconds = 1000; // Reduced from 2000

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<StoreContext>();
            var scopedLogger = services.GetRequiredService<ILogger<Program>>();

            scopedLogger.LogInformation("Database initialization attempt {Attempt} of {MaxRetries}", attempt, maxRetries);

            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new Exception("Cannot connect to database");
            }

            scopedLogger.LogInformation("Database connection successful");

            // ✅ Only run migrations if there are pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                scopedLogger.LogInformation("Pending migrations found: {Count}. Running migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
                scopedLogger.LogInformation("Database migration completed successfully");
            }
            else
            {
                scopedLogger.LogInformation("No pending migrations. Database is up to date.");
            }

            // ✅ Only seed if Products table is empty
            if (!await context.Products.AnyAsync())
            {
                scopedLogger.LogInformation("Database is empty. Starting seeding...");
                await StoreContextSeed.SeedAsync(context, scopedLogger);
                scopedLogger.LogInformation("Database seeding completed successfully");
            }
            else
            {
                scopedLogger.LogInformation("Database already contains data. Skipping seeding.");
            }

            return;
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
