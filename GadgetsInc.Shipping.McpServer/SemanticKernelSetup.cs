using Microsoft.SemanticKernel;

namespace GadgetsInc.Shipping.McpServer;

public static class SemanticKernelSetup
{
    public static IKernelBuilder AddShippingKernel(this IKernelBuilder builder)
    {
        // Add any custom plugins or configurations here
        builder.Plugins.AddFromType<Tools.Math>();

        return builder;
    }
    
}