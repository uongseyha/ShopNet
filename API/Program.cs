using API.Middleware;
using API.Services;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Removes options not supported by StackExchange.Redis (e.g. abortConnection from Upstash)
static string StripUnsupportedRedisOptions(string connectionString)
{
    var stripped = System.Text.RegularExpressions.Regex.Replace(
        connectionString,
        @",?\s*abortConnection\s*=\s*(?:True|False)\s*,?",
        "",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    return System.Text.RegularExpressions.Regex.Replace(stripped, @",{2,}", ",").Trim();
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddCors();

// StackExchange.Redis does not support 'abortConnection'; strip it for Upstash/Azure-style strings
var redisConnectionString = StripUnsupportedRedisOptions(
    builder.Configuration.GetConnectionString("Redis") ?? throw new Exception("Cannot get redis connection string"));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuation = ConfigurationOptions.Parse(redisConnectionString, ignoreUnknown: true);
    return ConnectionMultiplexer.Connect(configuation);
});
builder.Services.AddSingleton<ICartService, CartService>();

// Redis distributed cache for product data (30-day expiry)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "ShopNet:";
});
builder.Services.AddScoped<ProductCacheService>();

// Add Response Caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseResponseCaching();

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>();
//app.MapHub<NotificationHub>("/hub/notifications");
app.MapFallbackToController("Index", "Fallback");

// Only run migrations in Development or when explicitly needed
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<StoreContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        await context.Database.MigrateAsync();
        await StoreContextSeed.SeedAsync(context, logger: app.Logger);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

app.Run();