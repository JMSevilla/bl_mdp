using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IronPdf;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.PdfGenerator;

public interface IPdfGenerator
{
    Task<MemoryStream> Generate(string htmlBody, Option<string> htmlHeader, Option<string> htmlFooter);
    Task<PdfDocument> GeneratePages(IList<byte[]> arrays);
    Task<byte[]> MergePdfs(byte[] array, PdfDocument pdf2);
    Task<string> Generate(string htmlBody);
}