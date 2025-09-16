using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using GadgetsInc.ApiService.Functions;

namespace GadgetsInc.ApiService.Services;

public static class SemanticKernelSetup
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = services.AddKernel();

        // For testing purposes, use mock service since Ollama might not be available
        // In production, this would connect to the actual Ollama service
        var useOllama = configuration.GetValue<bool>("UseOllama", false);
        
        if (useOllama)
        {
            // Add Ollama connector - using the chat model from AppHost
            var ollamaEndpoint = configuration.GetConnectionString("ollama") ?? "http://localhost:11434";
            kernelBuilder.AddOllamaChatCompletion(
                modelId: "llama3.2", // This should match the model in AppHost
                endpoint: new Uri(ollamaEndpoint));
        }
        else
        {
            // Use mock service for testing/demo purposes
            services.AddSingleton<IChatCompletionService, MockChatCompletionService>();
        }

        // Add our custom functions
        kernelBuilder.Plugins.AddFromType<ProductFunctions>();
        kernelBuilder.Plugins.AddFromType<CustomerServiceFunctions>();

        return services;
    }
}