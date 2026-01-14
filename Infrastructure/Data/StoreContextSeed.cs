using System.Text.Json;
using Core.Entities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext context, ILogger logger)
        {
            try
            {
                // Check if products already exist
                if (!context.Products.Any())
                {
                    var productsData = await File.ReadAllTextAsync("../Infrastructure/Data/SeedData/products.json");
                    
                    var products = JsonSerializer.Deserialize<List<Product>>(productsData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (products != null && products.Count > 0)
                    {
                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();
                        
                        logger.LogInformation($"Seeded {products.Count} products to database");
                    }
                }
                else
                {
                    logger.LogInformation("Products already exist in database. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }
    }
}