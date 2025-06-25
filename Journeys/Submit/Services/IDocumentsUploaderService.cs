using System;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface IDocumentsUploaderService
{
    Task<Either<Error, int>> UploadGenericRetirementDocuments(string businessGroup, string referenceNumber, string caseNumber, string journeyType, MemoryStream pdfStream, string fileName);
    Task<Either<Error, int>> UploadDBRetirementDocument(string businessGroup, string referenceNumber, string caseNumber, MemoryStream pdfStream, string fileName, Guid? gbgId = null);
    Task<Error?> UploadQuoteRequestSummary(string businessGroup, string referenceNumber, string caseNumber, string caseType, MemoryStream summaryPdf, string fileName);
    Task UploadNonCaseRetirementQuoteDocument(string businessGroup, string referenceNumber, MemoryStream memoryStream, int calcSystemHistorySeqno);
}