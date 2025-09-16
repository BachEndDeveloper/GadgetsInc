using Microsoft.SemanticKernel;

namespace GadgetsInc.Shipping.McpServer;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddShippingKernel(this IServiceCollection services)
    {
        var kernelBuilder = services.AddKernel();

        
        // Add any custom plugins or configurations here
        kernelBuilder.Plugins.AddFromType<Tools.Math>();

        return services;
    }
    
}