using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GadgetsInc.Shipping.McpServer.Tools;

/// <summary>
/// Shipping-related functions for the MCP server.
/// These functions provide access to shipping information, package tracking, and search capabilities.
/// </summary>
public class ShippingFunctions
{
    [KernelFunction, Description("Get shipping information by shipping ID. Returns detailed shipping status, tracking information, and delivery details.")]
    public static string GetShipping(int shippingId)
    {
        // Simulate shipping data retrieval
        var random = new Random(shippingId);
        var statuses = new[] { "Processing", "Shipped", "In Transit", "Out for Delivery", "Delivered", "Exception" };
        var carriers = new[] { "UPS", "FedEx", "DHL", "USPS" };
        var origins = new[] { "New York, NY", "Los Angeles, CA", "Chicago, IL", "Houston, TX", "Phoenix, AZ" };
        var destinations = new[] { "Seattle, WA", "Miami, FL", "Denver, CO", "Boston, MA", "Atlanta, GA" };

        var status = statuses[random.Next(statuses.Length)];
        var carrier = carriers[random.Next(carriers.Length)];
        var origin = origins[random.Next(origins.Length)];
        var destination = destinations[random.Next(destinations.Length)];
        var trackingNumber = $"{carrier.ToUpper()}{random.Next(100000000, 999999999)}";

        var shippedDate = DateTime.Now.AddDays(-random.Next(1, 10));
        var expectedDelivery = shippedDate.AddDays(random.Next(3, 7));

        return $"Shipping ID: {shippingId}\n" +
               $"Status: {status}\n" +
               $"Carrier: {carrier}\n" +
               $"Tracking Number: {trackingNumber}\n" +
               $"Origin: {origin}\n" +
               $"Destination: {destination}\n" +
               $"Shipped Date: {shippedDate:yyyy-MM-dd}\n" +
               $"Expected Delivery: {expectedDelivery:yyyy-MM-dd}\n" +
               $"Last Updated: {DateTime.Now:yyyy-MM-dd HH:mm}";
    }

    [KernelFunction, Description("Get package information by package ID. Returns package details including contents, weight, dimensions, and current location.")]
    public static string GetPackage(string packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return "Error: Package ID cannot be empty.";
        }

        // Simulate package data retrieval
        var random = new Random(packageId.GetHashCode());
        var packageTypes = new[] { "Electronics", "Clothing", "Books", "Home & Garden", "Automotive", "Health & Beauty" };
        var locations = new[] { "Warehouse A", "Distribution Center B", "Local Facility C", "Delivery Vehicle", "Customer" };

        var packageType = packageTypes[random.Next(packageTypes.Length)];
        var currentLocation = locations[random.Next(locations.Length)];
        var weight = System.Math.Round(random.NextDouble() * 50 + 0.5, 2); // 0.5 to 50.5 lbs
        var length = random.Next(6, 36); // 6 to 36 inches
        var width = random.Next(4, 24);  // 4 to 24 inches
        var height = random.Next(2, 18); // 2 to 18 inches

        return $"Package ID: {packageId}\n" +
               $"Package Type: {packageType}\n" +
               $"Weight: {weight} lbs\n" +
               $"Dimensions: {length}\" × {width}\" × {height}\"\n" +
               $"Current Location: {currentLocation}\n" +
               $"Insurance Value: ${random.Next(50, 2000)}\n" +
               $"Fragile: {(random.NextDouble() > 0.7 ? "Yes" : "No")}\n" +
               $"Last Scanned: {DateTime.Now.AddHours(-random.Next(1, 48)):yyyy-MM-dd HH:mm}";
    }

    [KernelFunction, Description("Search shipping and package records by search term. Returns matching shipments and packages based on tracking numbers, package IDs, destinations, or other criteria.")]
    public static string Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "Error: Search term cannot be empty.";
        }

        // Simulate search functionality
        var random = new Random(searchTerm.GetHashCode());
        var resultCount = random.Next(1, 6); // 1 to 5 results
        var results = new List<string>();

        for (int i = 0; i < resultCount; i++)
        {
            var isShipment = random.NextDouble() > 0.5;
            
            if (isShipment)
            {
                var shippingId = random.Next(1000, 9999);
                var status = new[] { "Processing", "Shipped", "In Transit", "Delivered" }[random.Next(4)];
                var destination = new[] { "New York", "Los Angeles", "Chicago", "Houston" }[random.Next(4)];
                
                results.Add($"Shipping #{shippingId} - Status: {status} - Destination: {destination}");
            }
            else
            {
                var packageId = $"PKG{random.Next(100000, 999999)}";
                var type = new[] { "Electronics", "Clothing", "Books" }[random.Next(3)];
                var location = new[] { "Warehouse", "In Transit", "Local Facility" }[random.Next(3)];
                
                results.Add($"Package {packageId} - Type: {type} - Location: {location}");
            }
        }

        return $"Search results for '{searchTerm}':\n\n" +
               string.Join("\n", results.Select((r, i) => $"{i + 1}. {r}")) +
               $"\n\nFound {resultCount} result(s)";
    }
}