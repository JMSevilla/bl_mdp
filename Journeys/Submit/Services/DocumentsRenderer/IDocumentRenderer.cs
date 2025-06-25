using System.IO;
using System.Threading.Tasks;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface IDocumentRenderer
{
    Task<(MemoryStream PdfStream, string FileName)> RenderGenericSummaryPdf(DocumentsRendererData documentsRendererData, string accessToken, string env);
    Task<(MemoryStream PdfStream, string FileName)> RenderDirectPdf(DocumentsRendererData documentsRendererData, string accessToken, string env);
    Task<(string EmailHtmlBody, string EmailSubject, string EmailFrom, string EmailTo)> RenderGenericJourneySummaryEmail(DocumentsRendererData documentsRendererData, string accessToken, string env);  
}