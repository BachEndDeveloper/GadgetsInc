using Microsoft.SemanticKernel;

namespace GadgetsInc.Shipping.McpServer;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddShippingKernel(this IServiceCollection services)
    {
        var kernelBuilder = services.AddKernel();

        // Add shipping-specific plugins
        kernelBuilder.Plugins.AddFromType<Tools.ShippingFunctions>();
        
        // Keep math functions for additional utility
        kernelBuilder.Plugins.AddFromType<Tools.Math>();

        return services;
    }
    
}