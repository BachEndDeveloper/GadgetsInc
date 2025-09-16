using Microsoft.SemanticKernel;
using GadgetsInc.ApiService.Functions;

namespace GadgetsInc.ApiService.Services;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        // Add Ollama connector - using the chat model from AppHost
        var ollamaEndpoint = configuration.GetConnectionString("ollama") ?? "http://localhost:11434";
        kernelBuilder.AddOllamaChatCompletion(
            modelId: "llama3.2", // This should match the model in AppHost
            endpoint: new Uri(ollamaEndpoint));

        // Add our custom functions
        kernelBuilder.Plugins.AddFromType<ProductFunctions>();
        kernelBuilder.Plugins.AddFromType<CustomerServiceFunctions>();

        return services;
    }
}