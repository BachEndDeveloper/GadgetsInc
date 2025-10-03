using Microsoft.SemanticKernel;

namespace GadgetsInc.ProductCatalog.MCP;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddProductCatalogKernel(this IServiceCollection services)
    {
        var kernelBuilder = services.AddKernel();

        // Add product catalog specific plugins
        kernelBuilder.Plugins.AddFromType<Tools.ProductCatalogFunctions>();

        return services;
    }
}