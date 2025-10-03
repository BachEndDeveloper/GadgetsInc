using GadgetsInc.ProductCatalog.MCP.Tools;

// Test the functions manually
var result1 = ProductCatalogFunctions.GetProduct(1001);
Console.WriteLine("GetProduct(1001) result:");
Console.WriteLine(result1.Substring(0, Math.Min(200, result1.Length)) + "...");
Console.WriteLine();

var result2 = ProductCatalogFunctions.SearchProduct("smartphone");
Console.WriteLine("SearchProduct('smartphone') result:");
Console.WriteLine(result2.Substring(0, Math.Min(200, result2.Length)) + "...");
Console.WriteLine();

var result3 = ProductCatalogFunctions.SearchTag("gaming");
Console.WriteLine("SearchTag('gaming') result:");
Console.WriteLine(result3.Substring(0, Math.Min(200, result3.Length)) + "...");
Console.WriteLine();

Console.WriteLine("All functions executed successfully!");
