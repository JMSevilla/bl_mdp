namespace WTW.MdpService.Journeys.Submit.Services;

public interface IDocumentsRendererDataFactory
{
    DocumentsRendererData CreateForSubmit(string journeyType, string businessGroup, string referenceNumber, string accessKey, string caseNumber);
    DocumentsRendererData CreateForDirectPdfDownload(string journeyType, string templateKey, string businessGroup, string referenceNumber, string accessKey);
    DocumentsRendererData CreateForQuoteRequest(string businessGroup, string referenceNumber, string caseNumber, string accessKey, string quoteRequestCaseType);
}