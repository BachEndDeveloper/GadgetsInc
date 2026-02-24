using GadgetsInc.ApiService.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
    app.MapOpenApi();
    app.MapScalarApiReference();
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
                           You are a helpful customer service assistant for GadgetsInc, a technology company that sells smartphones, laptops, smartwatches, headphones, and tablets.
                           
                           You can help customers with:
                           - Product information and recommendations
                           - Order tracking and shipping information  
                           - Customer support and warranty questions
                           - Technical support and troubleshooting
                           
                           Always be polite, helpful, and professional. 
                           Use the available functions to provide accurate information.
                           Only respond with information you know to be correct and where a function is available.
                           If you don't have specific information, direct customers to contact support at 1-800-GADGETS.
                           """;

        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddUserMessage(request.Message);

        PromptExecutionSettings promptSettings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        try
        {
            var response =
                await chatService.GetChatMessageContentAsync(chatHistory, executionSettings: promptSettings,
                    kernel: kernel);
            
            return Results.Ok(new { response = response.Content });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, title: "Chat Completion Error");
        }

        
    })
    .WithName("SimpleChat");

const long MaxSummaryUploadSizeBytes = 10 * 1024 * 1024; // 10 MB

// Summary endpoint - upload a document and get an LLM-generated summary
app.MapPost("/summary", async (IFormFile file, Kernel kernel) =>
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "No file was uploaded." });

        if (file.Length > MaxSummaryUploadSizeBytes)
            return Results.BadRequest(new { error = "The uploaded file is too large. The maximum allowed size is 10 MB." });
        string documentText;
        try
        {
            documentText = await DocumentExtractionService.ExtractTextAsync(file);
        }
        catch (NotSupportedException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        if (string.IsNullOrWhiteSpace(documentText))
            return Results.BadRequest(new { error = "The uploaded document contains no readable text." });

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var systemPrompt = """
                           You are a document summarization assistant.
                           Your task is to produce a clear, concise summary of the document provided by the user.
                           - Capture the main topics, key points, and any important conclusions.
                           - Keep the summary factual and objective.
                           - Format the summary in plain prose unless bullet points would improve clarity.
                           """;

        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddUserMessage($"Please summarize the following document:\n\n{documentText}");

        try
        {
            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
            var summary = response.Content ?? string.Empty;
            return Results.Ok(new { summary });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, title: "Summary Error");
        }
    })
    .WithName("SummarizeDocument")
    .DisableAntiforgery();

// Compliance endpoint - upload a document and get a PII/GDPR compliance check
app.MapPost("/compliance", async (IFormFile file, Kernel kernel) =>
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "No file was uploaded." });

        if (file.Length > MaxSummaryUploadSizeBytes)
            return Results.BadRequest(new { error = "The uploaded file is too large. The maximum allowed size is 10 MB." });

        string documentText;
        try
        {
            documentText = await DocumentExtractionService.ExtractTextAsync(file);
        }
        catch (NotSupportedException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        if (string.IsNullOrWhiteSpace(documentText))
            return Results.BadRequest(new { error = "The uploaded document contains no readable text." });

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var systemPrompt = """
                           You are a data privacy and compliance expert specializing in GDPR and PII (Personally Identifiable Information).
                           Your task is to analyse the document provided by the user and identify any potential privacy or compliance concerns.

                           Look for and report on:
                           - PII such as names, email addresses, phone numbers, physical addresses, national ID / passport numbers, dates of birth, IP addresses, and financial data.
                           - Sensitive special-category data (health, biometric, racial/ethnic origin, political opinions, religious beliefs, trade union membership, sexual orientation).
                           - Potential GDPR violations, such as lack of consent indication, data retention issues, or cross-border transfer concerns.

                           Return ONLY valid JSON matching this schema:
                           {
                             "summaryTable": [
                               {
                                 "category": "PII|GDPR",
                                 "finding": "what was identified",
                                 "risk": "why this is a risk",
                                 "remediation": "recommended action"
                               }
                             ],
                             "PII_Risk": true|false,
                             "GDPR_Risk": true|false
                           }

                           Rules:
                           - Do not include markdown, code fences, or extra commentary.
                           - summaryTable may be empty if no issues are found.
                           - PII_Risk and GDPR_Risk must indicate whether there is a reasonable risk.
                           """;

        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddUserMessage($"Please check the following document for PII and GDPR compliance issues:\n\n{documentText}");

        try
        {
            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: kernel);
            var rawJson = response.Content?.Trim();
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return Results.Problem(detail: "Compliance model returned an empty response.", title: "Compliance Check Error");
            }

            ComplianceResponseDto? complianceResponse;
            try
            {
                complianceResponse = JsonSerializer.Deserialize<ComplianceResponseDto>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                return Results.Problem(detail: $"Compliance model returned invalid JSON: {ex.Message}", title: "Compliance Check Error");
            }

            if (complianceResponse is null)
            {
                return Results.Problem(detail: "Compliance model response could not be parsed.", title: "Compliance Check Error");
            }

            return Results.Ok(complianceResponse);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, title: "Compliance Check Error");
        }
    })
    .WithName("ComplianceCheck")
    .DisableAntiforgery();

app.MapDefaultEndpoints();

app.Run();

// Request models
public record ChatMessage(string Role, string Content);

public record ChatRequest(List<ChatMessage> Messages);

public record SimpleChatRequest(string Message);

public record ComplianceFindingDto(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("finding")] string Finding,
    [property: JsonPropertyName("risk")] string Risk,
    [property: JsonPropertyName("remediation")] string Remediation);

public record ComplianceResponseDto(
    [property: JsonPropertyName("summaryTable")] List<ComplianceFindingDto> SummaryTable,
    [property: JsonPropertyName("PII_Risk")] bool PiiRisk,
    [property: JsonPropertyName("GDPR_Risk")] bool GdprRisk);
