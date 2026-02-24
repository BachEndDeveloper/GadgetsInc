using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace GadgetsInc.ApiService.Services;

public static class DocumentExtractionService
{
    public static async Task<string> ExtractTextAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        using var stream = file.OpenReadStream();

        return extension switch
        {
            ".pdf" => await ExtractFromPdfAsync(stream),
            ".docx" => await ExtractFromDocxAsync(stream),
            ".txt" or ".text" => await ExtractFromTextAsync(stream),
            _ => throw new NotSupportedException($"File type '{extension}' is not supported. Supported types: .pdf, .docx, .txt")
        };
    }

    private static async Task<string> ExtractFromPdfAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var document = PdfDocument.Open(memoryStream);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }

    private static async Task<string> ExtractFromDocxAsync(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var wordDoc = WordprocessingDocument.Open(memoryStream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body is null)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            sb.AppendLine(paragraph.InnerText);
        }

        return sb.ToString();
    }

    private static async Task<string> ExtractFromTextAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
