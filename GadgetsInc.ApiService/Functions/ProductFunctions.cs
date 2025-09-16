using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GadgetsInc.ApiService.Functions;

public class ProductFunctions
{
    [KernelFunction, Description("Get information about GadgetsInc products. Provides details about available gadgets and their features.")]
    public static string GetProductInfo(string productName)
    {
        var products = new Dictionary<string, string>
        {
            { "smartphone", "GadgetsInc Smartphone X1 - Latest 5G smartphone with AI-powered camera and 48-hour battery life. Price: $899" },
            { "laptop", "GadgetsInc Laptop Pro - High-performance laptop with 16GB RAM, 1TB SSD, and 15-hour battery. Price: $1,299" },
            { "smartwatch", "GadgetsInc Watch Elite - Fitness tracking smartwatch with health monitoring and GPS. Price: $399" },
            { "headphones", "GadgetsInc Audio Pro - Wireless noise-canceling headphones with premium sound quality. Price: $249" },
            { "tablet", "GadgetsInc Tablet Max - 12-inch tablet with stylus support and all-day battery. Price: $699" }
        };

        var key = productName.ToLowerInvariant();
        if (products.TryGetValue(key, out var productInfo))
        {
            return productInfo;
        }

        return $"Sorry, I couldn't find information about '{productName}'. Available products: {string.Join(", ", products.Keys)}";
    }

    [KernelFunction, Description("Calculate shipping cost based on product weight and destination. Returns estimated shipping cost in USD.")]
    public static decimal CalculateShipping(double weightInKg, string destination)
    {
        var baseRate = destination.ToLowerInvariant() switch
        {
            "domestic" or "usa" or "us" => 5.99m,
            "canada" => 12.99m,
            "europe" => 19.99m,
            "asia" => 24.99m,
            _ => 29.99m // International
        };

        var weightCost = (decimal)(weightInKg * 2.50);
        return baseRate + weightCost;
    }

    [KernelFunction, Description("Check product availability and stock status. Returns current stock information.")]
    public static string CheckStock(string productName)
    {
        var stockInfo = new Dictionary<string, (int stock, string status)>
        {
            { "smartphone", (156, "In Stock") },
            { "laptop", (43, "In Stock") },
            { "smartwatch", (892, "In Stock") },
            { "headphones", (234, "In Stock") },
            { "tablet", (67, "In Stock") }
        };

        var key = productName.ToLowerInvariant();
        if (stockInfo.TryGetValue(key, out var info))
        {
            return $"{productName}: {info.stock} units {info.status}";
        }

        return $"Product '{productName}' not found in inventory system.";
    }
}