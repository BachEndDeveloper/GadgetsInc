using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GadgetsInc.ProductCatalog.MCP.Tools;

/// <summary>
/// Product catalog functions for the MCP server.
/// These functions provide access to product information, search capabilities, and tag-based filtering.
/// </summary>
public class ProductCatalogFunctions
{
    // Static product data for demonstration
    private static readonly Dictionary<int, Product> Products = new()
    {
        { 1001, new Product(1001, "GadgetsInc Smartphone X1", "Latest 5G smartphone with AI-powered camera and 48-hour battery life", 899.00m, new[] { "smartphone", "5G", "AI", "camera", "mobile" }, "Electronics") },
        { 1002, new Product(1002, "GadgetsInc Laptop Pro", "High-performance laptop with 16GB RAM, 1TB SSD, and 15-hour battery", 1299.00m, new[] { "laptop", "high-performance", "RAM", "SSD", "portable" }, "Electronics") },
        { 1003, new Product(1003, "GadgetsInc Watch Elite", "Fitness tracking smartwatch with health monitoring and GPS", 399.00m, new[] { "smartwatch", "fitness", "health", "GPS", "wearable" }, "Wearables") },
        { 1004, new Product(1004, "GadgetsInc Audio Pro", "Wireless noise-canceling headphones with premium sound quality", 249.00m, new[] { "headphones", "wireless", "noise-canceling", "audio", "music" }, "Audio") },
        { 1005, new Product(1005, "GadgetsInc Tablet Max", "12-inch tablet with stylus support and all-day battery", 699.00m, new[] { "tablet", "stylus", "productivity", "portable", "touch" }, "Electronics") },
        { 1006, new Product(1006, "GadgetsInc Camera 4K", "Professional 4K camera with advanced stabilization", 1599.00m, new[] { "camera", "4K", "professional", "stabilization", "photography" }, "Photography") },
        { 1007, new Product(1007, "GadgetsInc Speaker Mesh", "Smart home speaker with voice assistant integration", 199.00m, new[] { "speaker", "smart-home", "voice-assistant", "audio", "home" }, "Smart Home") },
        { 1008, new Product(1008, "GadgetsInc Gaming Mouse", "High-precision gaming mouse with customizable RGB", 89.00m, new[] { "mouse", "gaming", "precision", "RGB", "accessories" }, "Gaming") },
        { 1009, new Product(1009, "GadgetsInc Drone Sky", "Consumer drone with 4K camera and 30-minute flight time", 899.00m, new[] { "drone", "4K", "camera", "flight", "aerial" }, "Drones") },
        { 1010, new Product(1010, "GadgetsInc Charger Ultra", "Fast wireless charger compatible with all devices", 59.00m, new[] { "charger", "wireless", "fast-charging", "universal", "accessories" }, "Accessories") }
    };

    [KernelFunction, Description("Get product information by product number. Returns detailed product information including name, description, price, category, and tags.")]
    public static string GetProduct(int productNo)
    {
        try
        {
            if (productNo <= 0)
            {
                return "Error: Product number must be a positive integer.";
            }

            if (Products.TryGetValue(productNo, out var product))
            {
                return $"Product Number: {product.ProductNumber}\n" +
                       $"Name: {product.Name}\n" +
                       $"Description: {product.Description}\n" +
                       $"Price: ${product.Price:F2}\n" +
                       $"Category: {product.Category}\n" +
                       $"Tags: {string.Join(", ", product.Tags)}\n" +
                       $"In Stock: Yes\n" +
                       $"Last Updated: {DateTime.Now:yyyy-MM-dd HH:mm}";
            }

            return $"Error: Product with number {productNo} not found. Available product numbers: {string.Join(", ", Products.Keys.OrderBy(k => k))}";
        }
        catch (Exception ex)
        {
            return $"Error retrieving product {productNo}: {ex.Message}";
        }
    }

    [KernelFunction, Description("Search products by search term. Returns matching products based on name, description, or category. Supports partial matching and case-insensitive search.")]
    public static string SearchProduct(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return "Error: Search term cannot be empty.";
            }

            var searchTermLower = searchTerm.ToLowerInvariant();
            var matchingProducts = Products.Values
                .Where(p => p.Name.ToLowerInvariant().Contains(searchTermLower) ||
                           p.Description.ToLowerInvariant().Contains(searchTermLower) ||
                           p.Category.ToLowerInvariant().Contains(searchTermLower))
                .OrderBy(p => p.ProductNumber)
                .ToList();

            if (matchingProducts.Count == 0)
            {
                return $"No products found matching '{searchTerm}'. Try searching for terms like 'smartphone', 'laptop', 'camera', or 'wireless'.";
            }

            var results = matchingProducts.Select((p, i) => 
                $"{i + 1}. Product #{p.ProductNumber}: {p.Name} - ${p.Price:F2} ({p.Category})")
                .ToList();

            return $"Search results for '{searchTerm}':\n\n" +
                   string.Join("\n", results) +
                   $"\n\nFound {matchingProducts.Count} product(s). Use GetProduct() with the product number for detailed information.";
        }
        catch (Exception ex)
        {
            return $"Error searching products with term '{searchTerm}': {ex.Message}";
        }
    }

    [KernelFunction, Description("Search products by tag. Returns products that match the specified tag. Tags include categories like 'smartphone', 'wireless', 'gaming', etc.")]
    public static string SearchTag(string searchterm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchterm))
            {
                return "Error: Search tag cannot be empty.";
            }

            var searchTagLower = searchterm.ToLowerInvariant();
            var matchingProducts = Products.Values
                .Where(p => p.Tags.Any(tag => tag.ToLowerInvariant().Contains(searchTagLower)))
                .OrderBy(p => p.ProductNumber)
                .ToList();

            if (matchingProducts.Count == 0)
            {
                // Get all unique tags for suggestion
                var allTags = Products.Values
                    .SelectMany(p => p.Tags)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                return $"No products found with tag '{searchterm}'. Available tags: {string.Join(", ", allTags)}";
            }

            var results = matchingProducts.Select((p, i) => 
                $"{i + 1}. Product #{p.ProductNumber}: {p.Name} - ${p.Price:F2}\n" +
                $"   Tags: {string.Join(", ", p.Tags.Where(t => t.ToLowerInvariant().Contains(searchTagLower)))}")
                .ToList();

            return $"Products with tag '{searchterm}':\n\n" +
                   string.Join("\n\n", results) +
                   $"\n\nFound {matchingProducts.Count} product(s). Use GetProduct() with the product number for detailed information.";
        }
        catch (Exception ex)
        {
            return $"Error searching products by tag '{searchterm}': {ex.Message}";
        }
    }
}

/// <summary>
/// Product data model
/// </summary>
public record Product(
    int ProductNumber,
    string Name,
    string Description,
    decimal Price,
    string[] Tags,
    string Category
);