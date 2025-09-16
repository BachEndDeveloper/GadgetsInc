using System.Data.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using GadgetsInc.ApiService.Functions;

namespace GadgetsInc.ApiService.Services;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();
        
        // Add Ollama connector - using the chat model from AppHost
        DbConnectionStringBuilder dbConnectionStringBuilder = new DbConnectionStringBuilder();
        dbConnectionStringBuilder.ConnectionString =
            configuration.GetConnectionString("chat") ?? "http://localhost:11434";
        dbConnectionStringBuilder.TryGetValue("endpoint", out var uri);
        dbConnectionStringBuilder.TryGetValue("model", out var model);
        kernelBuilder.AddOllamaChatCompletion(
            modelId: (model?.ToString() ?? string.Empty), // This should match the model in AppHost
            endpoint: new Uri(uri?.ToString() ?? string.Empty));

        // Add our custom functions
        kernelBuilder.Plugins.AddFromType<ProductFunctions>();
        kernelBuilder.Plugins.AddFromType<CustomerServiceFunctions>();

        return services;
    }
}