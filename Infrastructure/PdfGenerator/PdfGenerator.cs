using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPdf;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.Infrastructure.PdfGenerator;

public class PdfGenerator : IPdfGenerator
{
    private const int _headerFooterMaxHeight = 25; //millimeters   
    private readonly ILogger<PdfGenerator> _logger;
    public PdfGenerator(ILogger<PdfGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<MemoryStream> Generate(string htmlBody, Option<string> htmlHeader, Option<string> htmlFooter)
    {
        _logger.LogInformation("Generating PDF...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var pdf = await ChromePdfRenderer.StaticRenderHtmlAsPdfAsync(htmlBody);

        htmlHeader.IfSome(x => pdf.AddHtmlHeaders(new HtmlHeaderFooter { HtmlFragment = x, MaxHeight = _headerFooterMaxHeight }));
        htmlFooter.IfSome(x => pdf.AddHtmlFooters(new HtmlHeaderFooter { HtmlFragment = x, MaxHeight = _headerFooterMaxHeight }));
        stopwatch.Stop();

        var streamSize = pdf.Stream.Length;
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation("PDF generated successfully in {elapsedMilliseconds} milliseconds. Stream size: {streamSize} bytes", elapsedMilliseconds, streamSize);

        return pdf.Stream;
    }

    public async Task<string> Generate(string htmlBody)
    {
        var pdf = await ChromePdfRenderer.StaticRenderHtmlAsPdfAsync(htmlBody);
        pdf.AddHtmlHeaders(new HtmlHeaderFooter { MaxHeight = _headerFooterMaxHeight });
        pdf.AddHtmlFooters(new HtmlHeaderFooter { MaxHeight = _headerFooterMaxHeight });
        var ByteArray = pdf.BinaryData;
        var Base64Result = Convert.ToBase64String(ByteArray);
        return Base64Result;
    }

    public async Task<PdfDocument> GeneratePages(IList<byte[]> arrays)
    {
        if (!arrays.Any())
            return null;

        var pdf = new PdfDocument(arrays[0]);
        for (int i = 1; i < arrays.Count(); i++)
        {
            var page = new PdfDocument(arrays[i]);
            pdf.AppendPdf(page);
        }

        return pdf;
    }

    public async Task<byte[]> MergePdfs(byte[] array, PdfDocument pdf2)
    {
        var pdf1 = new PdfDocument(array);
        PdfDocument combinedPdf = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2 });
        return combinedPdf.BinaryData;
    }
}