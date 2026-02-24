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
        // Open directly from a seekable stream to avoid double-buffering.
        // Fall back to a MemoryStream copy only when the stream is not seekable.
        Stream docxStream;
        MemoryStream? owned = null;
        if (stream.CanSeek)
        {
            docxStream = stream;
        }
        else
        {
            owned = new MemoryStream();
            await stream.CopyToAsync(owned);
            owned.Position = 0;
            docxStream = owned;
        }

        using var wordDoc = WordprocessingDocument.Open(docxStream, false);
        var sb = new StringBuilder();
        foreach (var paragraph in wordDoc.MainDocumentPart?.Document?.Body?.Descendants<Paragraph>() ?? [])
        {
            sb.AppendLine(paragraph.InnerText);
        }

        owned?.Dispose();
        return sb.ToString();
    }

    private static async Task<string> ExtractFromTextAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
