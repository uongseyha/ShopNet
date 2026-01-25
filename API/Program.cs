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
   
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Redis") 
        ?? throw new InvalidOperationException("Redis connection string not found");
    var configuration = ConfigurationOptions.Parse(connString, true);
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddCors();

//For published
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("https://shopnet2k6.azurewebsites.net")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    var scopedLogger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        scopedLogger.LogInformation("Starting database migration...");
        await context.Database.MigrateAsync();
        scopedLogger.LogInformation("Database migration completed successfully");
        
        scopedLogger.LogInformation("Starting database seeding...");
        await StoreContextSeed.SeedAsync(context, scopedLogger);
        scopedLogger.LogInformation("Database seeding completed successfully");
    }
    catch (Exception ex)
    {
        scopedLogger.LogError(ex, "An error occurred during migration or seeding");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseCors("AllowAngular");

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

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

logger.LogInformation("Application started successfully");

app.Run();
