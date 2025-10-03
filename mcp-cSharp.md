# MCP Server Best Practices for C# and .NET

This document provides best practices and guidance for implementing Model Context Protocol (MCP) servers using C# and .NET, with a focus on Semantic Kernel integration.

## Overview

The Model Context Protocol (MCP) provides a standardized way for applications to integrate with external tools and data sources. When implementing MCP servers in C#/.NET, we leverage the official .NET SDK and integrate with Microsoft's Semantic Kernel for robust AI-powered functionality.

## Prerequisites

- .NET 9.0 SDK or later
- `ModelContextProtocol-SemanticKernel` NuGet package
- `Microsoft.SemanticKernel` NuGet package

## Project Structure

```
MyMcpServer/
├── Program.cs                 # Main application entry point
├── Tools/                     # MCP functions organized by domain
│   ├── MyDomainFunctions.cs   # Domain-specific function implementations
│   └── UtilityFunctions.cs    # Generic utility functions
├── Extensions/                # Extension methods for configuration
│   └── McpServerBuilderExtensions.cs
└── Configuration/             # Kernel and service setup
    └── SemanticKernelSetup.cs
```

## Core Components

### 1. Function Implementation

MCP functions should be implemented as static methods decorated with `[KernelFunction]` and `[Description]` attributes:

```csharp
using System.ComponentModel;
using Microsoft.SemanticKernel;

public class ShippingFunctions
{
    [KernelFunction, Description("Get shipping information by shipping ID. Returns detailed shipping status and tracking information.")]
    public static string GetShipping(int shippingId)
    {
        // Implementation here
        return $"Shipping details for ID: {shippingId}";
    }

    [KernelFunction, Description("Search shipping records by search term. Supports tracking numbers, destinations, and package IDs.")]
    public static string Search(string searchTerm)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return "Error: Search term cannot be empty.";
        }
        
        // Implementation here
        return $"Search results for: {searchTerm}";
    }
}
```

### 2. Semantic Kernel Configuration

Set up Semantic Kernel with your MCP functions in a dedicated configuration class:

```csharp
using Microsoft.SemanticKernel;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddMyKernel(this IServiceCollection services)
    {
        var kernelBuilder = services.AddKernel();

        // Register function plugins
        kernelBuilder.Plugins.AddFromType<MyDomainFunctions>();
        kernelBuilder.Plugins.AddFromType<UtilityFunctions>();

        return services;
    }
}
```

### 3. Server Setup

Configure the MCP server in `Program.cs`:

```csharp
using MyMcpServer;
using MyMcpServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();

// Add Semantic Kernel with your functions
builder.Services.AddMyKernel();

// Configure MCP Server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()  // Standard MCP transport
    .WithTools();                // Auto-register Semantic Kernel functions

var app = builder.Build();

// Health check for monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
```

## Best Practices

### Function Design

1. **Clear Descriptions**: Use descriptive `[Description]` attributes that explain:
   - What the function does
   - What parameters it accepts
   - What it returns
   - Any important constraints or behavior

2. **Input Validation**: Always validate input parameters:
   ```csharp
   public static string MyFunction(string input)
   {
       if (string.IsNullOrWhiteSpace(input))
       {
           return "Error: Input cannot be empty.";
       }
       // ... rest of implementation
   }
   ```

3. **Consistent Return Formats**: Return structured, human-readable strings:
   ```csharp
   return $"Package ID: {packageId}\n" +
          $"Status: {status}\n" +
          $"Location: {location}\n" +
          $"Last Updated: {lastUpdated:yyyy-MM-dd HH:mm}";
   ```

4. **Error Handling**: Return user-friendly error messages instead of throwing exceptions:
   ```csharp
   try
   {
       // Function logic
       return result;
   }
   catch (Exception ex)
   {
       return $"Error: {ex.Message}";
   }
   ```

### Performance Considerations

1. **Async Operations**: For I/O-bound operations, consider async patterns:
   ```csharp
   [KernelFunction, Description("Async function example")]
   public static async Task<string> GetDataAsync(string id)
   {
       // Use async operations for database calls, HTTP requests, etc.
       var result = await SomeAsyncOperation(id);
       return result;
   }
   ```

2. **Caching**: Implement caching for frequently accessed data:
   ```csharp
   private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
   
   public static string GetCachedData(string key)
   {
       if (_cache.TryGetValue(key, out string cachedResult))
       {
           return cachedResult;
       }
       
       var result = ExpensiveOperation(key);
       _cache.Set(key, result, TimeSpan.FromMinutes(5));
       return result;
   }
   ```

### Security Best Practices

1. **Input Sanitization**: Sanitize all input parameters
2. **Rate Limiting**: Implement rate limiting for resource-intensive operations
3. **Authentication**: If needed, implement proper authentication mechanisms
4. **Logging**: Use structured logging for debugging and monitoring:
   ```csharp
   private static readonly ILogger _logger = LoggerFactory.Create(builder => 
       builder.AddConsole()).CreateLogger<MyFunctions>();

   public static string MyFunction(string input)
   {
       _logger.LogInformation("Processing request with input: {Input}", input);
       // ... implementation
   }
   ```

### Testing

1. **Unit Tests**: Test individual functions in isolation:
   ```csharp
   [Test]
   public void GetShipping_ValidId_ReturnsExpectedFormat()
   {
       var result = ShippingFunctions.GetShipping(12345);
       
       Assert.That(result, Does.Contain("Shipping ID: 12345"));
       Assert.That(result, Does.Contain("Status:"));
       Assert.That(result, Does.Contain("Tracking Number:"));
   }
   ```

2. **Integration Tests**: Test the MCP server end-to-end with a test client

### Deployment

1. **Docker**: Create a Dockerfile for containerized deployment:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:9.0
   WORKDIR /app
   COPY bin/Release/net9.0/publish .
   ENTRYPOINT ["dotnet", "MyMcpServer.dll"]
   ```

2. **Health Checks**: Always include health check endpoints for monitoring

3. **Configuration**: Use ASP.NET Core configuration for environment-specific settings

## Advanced Patterns

### Custom Tool Registration

For more control over tool registration:

```csharp
public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithCustomTools(this IMcpServerBuilder builder)
    {
        // Custom tool registration logic
        builder.Services.AddSingleton<IEnumerable<McpServerTool>>(services =>
        {
            var tools = new List<McpServerTool>();
            
            // Add specific functions as tools
            var kernel = services.GetRequiredService<Kernel>();
            foreach (var plugin in kernel.Plugins)
            {
                foreach (var function in plugin)
                {
                    tools.Add(McpServerTool.Create(function));
                }
            }
            
            return tools;
        });

        return builder;
    }
}
```

### Dependency Injection

Leverage .NET's built-in DI container for services:

```csharp
public class DatabaseService
{
    public string GetData(string id) => $"Data for {id}";
}

public class MyFunctions
{
    private static DatabaseService? _dbService;
    
    public static void Initialize(DatabaseService dbService)
    {
        _dbService = dbService;
    }
    
    [KernelFunction]
    public static string GetDatabaseData(string id)
    {
        return _dbService?.GetData(id) ?? "Service not initialized";
    }
}
```

## Troubleshooting

### Common Issues

1. **Function Not Available**: Ensure functions are properly registered with `[KernelFunction]`
2. **Transport Issues**: Verify stdio transport is correctly configured
3. **Serialization Problems**: Use simple types for parameters and return values

### Debugging

Enable detailed logging to troubleshoot MCP server issues:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ModelContextProtocol": "Debug"
    }
  }
}
```

## Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/docs)
- [Microsoft Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel)
- [.NET MCP SDK Documentation](https://learn.microsoft.com/en-us/dotnet/ai/get-started-mcp)
- [MCP C# SDK GitHub Repository](https://github.com/modelcontextprotocol/csharp-sdk)

## Example Implementation

See the `GadgetsInc.Shipping.McpServer` project in this repository for a complete working example that implements:

- Multiple shipping-related MCP functions
- Proper input validation and error handling
- Structured return formats
- Integration with Semantic Kernel
- Health check endpoints
- Clean separation of concerns

This example demonstrates all the best practices outlined in this document and serves as a reference implementation for building robust MCP servers in C#/.NET.