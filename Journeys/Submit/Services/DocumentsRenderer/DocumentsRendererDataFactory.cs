using System;
using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Journeys.Submit.Services;

public class DocumentsRendererDataFactory : IDocumentsRendererDataFactory
{
    private readonly IEnumerable<string> _supportedJourneyTypes = new List<string> { "dcretirementapplication", "requestquote", "dbcoreretirementapplication", "dbretirementapplication" };

    public DocumentsRendererData CreateForQuoteRequest(string businessGroup, string referenceNumber, string caseNumber, string accessKey, string quoteRequestCaseType)
    {
        string templateKey;
        if (quoteRequestCaseType.Equals("retirement", StringComparison.InvariantCultureIgnoreCase))
            templateKey = "request_retirement_quote_pdf";
        else if (quoteRequestCaseType.Equals("transfer", StringComparison.InvariantCultureIgnoreCase))
            templateKey = "request_transfer_quote_pdf";
        else
            throw new ArgumentException($"No template key exists for the Quote request case type: {quoteRequestCaseType}.");

        return new DocumentsRendererData(businessGroup, referenceNumber, caseNumber, accessKey, templateKey, "quote_request_submission_email", "retirement_quote_data_summary", "requestquote");
    }

    public DocumentsRendererData CreateForSubmit(string journeyType, string businessGroup, string referenceNumber, string accessKey, string caseNumber)
    {
        if (!_supportedJourneyTypes.Contains(journeyType, StringComparer.InvariantCultureIgnoreCase)
            || string.Equals(journeyType, "requestquote", StringComparison.InvariantCultureIgnoreCase))
            throw new ArgumentException($"Unsupported journey type: {journeyType}.");

        var pdfTemplateKey = journeyType.ToLower() switch
        {
            "dcretirementapplication" => "dc_retirement_application_pdf",
            "dbcoreretirementapplication" => "dbcoreretirementapplication_pdf",
            "dbretirementapplication" => "db_retirement_application_pdf"
        };

        var emailTemplateKey = journeyType switch
        {
            "dcretirementapplication" => "dc_retirement_submission_email",
            "dbcoreretirementapplication" => "dbcoreretirementapplication_submission_email",
            _ => default(string),
        };

        var dataSummaryBlockKey = journeyType switch
        {
            "dcretirementapplication" => "dc_retirement_application",
            "dbcoreretirementapplication" => "dbcoreretirementapplication_data_summary",
            "dbretirementapplication" => "db_retirement_application",
            _ => default(string),
        };

        return new DocumentsRendererData(businessGroup, referenceNumber, caseNumber, accessKey, pdfTemplateKey, emailTemplateKey, dataSummaryBlockKey, journeyType);
    }

    public DocumentsRendererData CreateForDirectPdfDownload(string journeyType, string templateKey, string businessGroup, string referenceNumber, string accessKey)
    {
        string templateKeyPrefix = templateKey.Contains("_pdf") 
            ? templateKey.Substring(0, templateKey.LastIndexOf("_pdf")) 
            : templateKey;
        return new DocumentsRendererData(businessGroup, referenceNumber, null, accessKey, templateKey, null, templateKeyPrefix, journeyType);
    }
}