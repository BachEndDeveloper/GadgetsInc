using GadgetsInc.ProductCatalog.MCP;
using GadgetsInc.ProductCatalog.MCP.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Add Semantic Kernel with product catalog functions
builder.Services.AddProductCatalogKernel();

// Configure MCP Server with stdio transport and tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Health check endpoint for monitoring
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.Run();
