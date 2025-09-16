using GadgetsInc.ApiService.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore-swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Semantic Kernel with AI functions
builder.Services.AddSemanticKernel(builder.Configuration);

// Add CORS for the web frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Enable CORS
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chat endpoint with streaming response
app.MapPost("/chat", async (ChatRequest request, Kernel kernel) =>
{
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    
    // Create system prompt
    var systemPrompt = """
        You are a helpful customer service assistant for GadgetsInc, a technology company that sells smartphones, laptops, smartwatches, headphones, and tablets.
        
        You can help customers with:
        - Product information and recommendations
        - Order tracking and shipping information  
        - Customer support and warranty questions
        - Technical support and troubleshooting
        
        Always be polite, helpful, and professional. Use the available functions to provide accurate information.
        If you don't have specific information, direct customers to contact support at 1-800-GADGETS.
        """;

    // Build chat history
    var chatHistory = new ChatHistory(systemPrompt);
    
    // Add conversation history
    foreach (var message in request.Messages)
    {
        if (message.Role == "user")
            chatHistory.AddUserMessage(message.Content);
        else if (message.Role == "assistant")
            chatHistory.AddAssistantMessage(message.Content);
    }

    // Create the streaming response
    return Results.Stream(async (stream) =>
    {
        try
        {
            var response = chatService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                kernel: kernel);

            await foreach (var chunk in response)
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    var jsonChunk = JsonSerializer.Serialize(new { content = chunk.Content });
                    var data = $"data: {jsonChunk}\n\n";
                    await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(data));
                    await stream.FlushAsync();
                }
            }
            
            // Send completion signal
            await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("data: [DONE]\n\n"));
            await stream.FlushAsync();
        }
        catch (Exception ex)
        {
            var errorData = JsonSerializer.Serialize(new { error = ex.Message });
            await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes($"data: {errorData}\n\n"));
            await stream.FlushAsync();
        }
    }, "text/plain; charset=utf-8");
})
.WithName("StreamChat");

// Simple chat endpoint for testing
app.MapPost("/chat/simple", async (SimpleChatRequest request, Kernel kernel) =>
{
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    
    var systemPrompt = """
        You are a helpful customer service assistant for GadgetsInc. Be polite and helpful.
        Use the available functions to provide accurate product and support information.
        """;

    var chatHistory = new ChatHistory(systemPrompt);
    chatHistory.AddUserMessage(request.Message);

    var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
    
    return Results.Ok(new { response = response.Content });
})
.WithName("SimpleChat");

app.MapDefaultEndpoints();

app.Run();

// Request models
public record ChatMessage(string Role, string Content);
public record ChatRequest(List<ChatMessage> Messages);
public record SimpleChatRequest(string Message);
