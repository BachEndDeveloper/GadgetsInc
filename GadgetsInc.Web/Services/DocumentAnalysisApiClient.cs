using System.Text.Json;
using System.Text.Json.Serialization;

namespace GadgetsInc.Web.Services;

public class DocumentAnalysisApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Task<string> UploadForSummaryAsync(Stream fileStream, string fileName, string? contentType, CancellationToken cancellationToken = default)
        => UploadAndReadAsync("/summary", "summary", fileStream, fileName, contentType, cancellationToken);

    public Task<ComplianceResponseDto> UploadForComplianceAsync(Stream fileStream, string fileName, string? contentType, CancellationToken cancellationToken = default)
        => UploadAndReadComplianceAsync("/compliance", fileStream, fileName, contentType, cancellationToken);

    private async Task<string> UploadAndReadAsync(string endpoint, string responseProperty, Stream fileStream, string fileName, string? contentType, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType = new(contentType);
        }

        form.Add(fileContent, "file", fileName);

        using var response = await httpClient.PostAsync(endpoint, form, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Request to {endpoint} failed with status {(int)response.StatusCode}: {responseBody}",
                null,
                response.StatusCode);
        }

        using var json = JsonDocument.Parse(responseBody);
        if (json.RootElement.TryGetProperty(responseProperty, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            return string.IsNullOrWhiteSpace(text)
                ? "The API returned an empty response."
                : text;
        }

        throw new InvalidOperationException($"The API response does not contain '{responseProperty}'.");
    }

    private async Task<ComplianceResponseDto> UploadAndReadComplianceAsync(string endpoint, Stream fileStream, string fileName, string? contentType, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            fileContent.Headers.ContentType = new(contentType);
        }

        form.Add(fileContent, "file", fileName);

        using var response = await httpClient.PostAsync(endpoint, form, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Request to {endpoint} failed with status {(int)response.StatusCode}: {responseBody}",
                null,
                response.StatusCode);
        }

        var result = JsonSerializer.Deserialize<ComplianceResponseDto>(responseBody, JsonOptions);
        if (result is null)
        {
            throw new InvalidOperationException("The API returned an empty compliance response.");
        }

        return result with
        {
            SummaryTable = result.SummaryTable ?? []
        };
    }
}

public record ComplianceFindingDto(
    string Category,
    string Finding,
    string Risk,
    string Remediation);

public record ComplianceResponseDto(
    List<ComplianceFindingDto> SummaryTable,
    [property: JsonPropertyName("PII_Risk")] bool PiiRisk,
    [property: JsonPropertyName("GDPR_Risk")] bool GdprRisk);
