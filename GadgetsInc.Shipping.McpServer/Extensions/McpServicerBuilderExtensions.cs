using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;

namespace GadgetsInc.Shipping.McpServer.Extensions;

public static class McpServicerBuilderExtensions
{
    public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, Kernel? kernel = null)
    {
        // If plugins are provided directly, add them as tools
        if (kernel is not null)
        {
            foreach (var plugin in kernel.Plugins)
            {
                foreach (var function in plugin)
                {
                    builder.Services.AddSingleton(McpServerTool.Create(function));
                }
            }

            return builder;
        }

        // If no plugins are provided explicitly, add all functions from the kernel plugins registered in DI container as tools
        builder.Services.AddSingleton<IEnumerable<McpServerTool>>(services =>
        {
            IEnumerable<KernelPlugin> plugins = services.GetServices<KernelPlugin>();

            List<McpServerTool> tools = new(plugins.Count());

            foreach (var plugin in plugins)
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