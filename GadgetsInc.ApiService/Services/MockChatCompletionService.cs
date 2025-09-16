using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using GadgetsInc.ApiService.Functions;

namespace GadgetsInc.ApiService.Services;

public class MockChatCompletionService : IChatCompletionService
{
    private readonly ProductFunctions _productFunctions = new();
    private readonly CustomerServiceFunctions _customerFunctions = new();

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        var response = GenerateResponse(lastMessage);
        
        await Task.Delay(500, cancellationToken); // Simulate AI processing time
        
        return new[] { new ChatMessageContent(AuthorRole.Assistant, response) };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        var response = GenerateResponse(lastMessage);
        
        // Simulate streaming by yielding chunks
        var words = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            await Task.Delay(50, cancellationToken); // Simulate typing speed
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
        }
    }

    private string GenerateResponse(string message)
    {
        message = message.ToLowerInvariant();

        // Check for product-related queries
        if (message.Contains("smartphone") || message.Contains("phone"))
        {
            return ProductFunctions.GetProductInfo("smartphone");
        }
        
        if (message.Contains("laptop"))
        {
            return ProductFunctions.GetProductInfo("laptop");
        }
        
        if (message.Contains("smartwatch") || message.Contains("watch"))
        {
            return ProductFunctions.GetProductInfo("smartwatch");
        }
        
        if (message.Contains("headphones"))
        {
            return ProductFunctions.GetProductInfo("headphones");
        }
        
        if (message.Contains("tablet"))
        {
            return ProductFunctions.GetProductInfo("tablet");
        }

        // Check for shipping queries
        if (message.Contains("shipping") && message.Contains("cost"))
        {
            var weight = 1.5; // Default weight
            var destination = "domestic";
            
            if (message.Contains("europe")) destination = "europe";
            if (message.Contains("canada")) destination = "canada";
            if (message.Contains("asia")) destination = "asia";
            
            // Extract weight if mentioned
            if (message.Contains("2kg") || message.Contains("2 kg")) weight = 2.0;
            if (message.Contains("3kg") || message.Contains("3 kg")) weight = 3.0;
            
            var cost = ProductFunctions.CalculateShipping(weight, destination);
            return $"Shipping cost for {weight}kg to {destination}: ${cost:F2}";
        }

        // Check for stock queries
        if (message.Contains("stock") || message.Contains("available"))
        {
            foreach (var product in new[] { "smartphone", "laptop", "smartwatch", "headphones", "tablet" })
            {
                if (message.Contains(product))
                {
                    return ProductFunctions.CheckStock(product);
                }
            }
        }

        // Check for support queries
        if (message.Contains("track") && message.Contains("order"))
        {
            return CustomerServiceFunctions.TrackOrder("ORD123456");
        }
        
        if (message.Contains("return") || message.Contains("warranty"))
        {
            return CustomerServiceFunctions.GetSupportInfo("warranty");
        }
        
        if (message.Contains("support") || message.Contains("help") || message.Contains("contact"))
        {
            return CustomerServiceFunctions.GetSupportInfo("contact");
        }

        // Default response
        return "Hello! I'm your GadgetsInc AI assistant. I can help you with:\n\n" +
               "• Product information (smartphones, laptops, smartwatches, headphones, tablets)\n" +
               "• Shipping cost calculations\n" +
               "• Stock availability\n" +
               "• Order tracking\n" +
               "• Customer support and warranty information\n\n" +
               "What would you like to know about?";
    }
}