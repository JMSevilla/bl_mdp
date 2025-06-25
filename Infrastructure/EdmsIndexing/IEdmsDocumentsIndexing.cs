using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.EdmsIndexing;

public interface IEdmsDocumentsIndexing
{
    Task<Either<Error, HttpStatusCode>> CleanAfterPostIndex(Guid gbgId);
    Task<Error?> PostIndexBereavement(string businessGroup, string caseNumber, int preIndexBatchNumber);
    Task<Error?> PostIndexRetirement(string referenceNumber, string businessGroup, string caseNumber, string caseCode, int preIndexBatchNumber);
    Task<Either<Error, EdmsPreIndexResult>> PreIndexRetirement(RetirementJourney journey, string referenceNumber, string businessGroup, MemoryStream summaryPdf);
    Task<Either<Error, List<(int ImageId, string DocUuid)>>> PostIndexTransferDocuments(string businessGroup, string referenceNumber, string caseNumber, TransferJourney journey, IList<UploadedDocument> documents);
    Task<Either<Error, List<(int imageId, string docUuid)>>> PostIndexBereavementDocuments(
        string businessGroup,
        string caseNumber,
        IList<UploadedDocument> documents);
}
