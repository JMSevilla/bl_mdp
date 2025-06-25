namespace WTW.MdpService.Journeys.Submit.Services;

public class DocumentsRendererData
{
    public DocumentsRendererData(string businessGroup,
        string referenceNumber,
        string caseNumber,
        string accessKey,
        string pdfSummaryTemplateKey,
        string emailTemplateKey,
        string dataSummaryBlockKey,
        string journeyType)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        CaseNumber = caseNumber;
        AccessKey = accessKey;
        PdfSummaryTemplateKey = pdfSummaryTemplateKey;
        EmailTemplateKey = emailTemplateKey;
        DataSummaryBlockKey = dataSummaryBlockKey;
        JourneyType = journeyType;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string CaseNumber { get; }
    public string PdfSummaryTemplateKey { get; }
    public string EmailTemplateKey { get; }
    public string DataSummaryBlockKey { get; }
    public string JourneyType { get; }
    public string AccessKey { get; }
}