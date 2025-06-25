using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Infrastructure.Edms;

public interface IEdmsClient
{
    Task<Either<Error, Stream>> GetDocumentOrError(int id);
    Task<Stream> GetDocument(int id);
    Task<ICollection<(int Id, Stream Stream)>> GetDocuments(ICollection<Domain.Members.Document> documents);

    Task<Either<DocumentUploadError, DocumentUploadResponse>> UploadDocument(
       string businessGroup,
       string fileName,
       Stream blob,
       int? batchNumber = null);

    Task<Either<PreindexError, PreindexResponse>> PreindexDocument(
        string businessGroup,
        string referenceNumber,
        string client,
        MemoryStream blob,
        int? batchNumber = null);

    Task<Either<PostIndexError, IndexResponse>> IndexRetirementDocument(
        string businessGroup,
        string referenceNumber,
        int batchNumber,
        string caseNumber,
        string caseCode);

    Task<Either<PostIndexError, IndexResponse>> IndexBereavementDocument(
        string businessGroup,
        int batchNumber,
        string caseNumber);

    Task<Either<PostIndexError, IndexResponse>> IndexDocument(
        string businessGroup,
        string referenceNumber,
        int batchNumber);

    Task<Either<PostIndexError, PostindexDocumentsResponse>> PostindexDocuments(
        string businessGroup,
        string referenceNumber,
        string caseNumber,
        IList<UploadedDocument> documents);

    Task<Either<PostIndexError, PostindexDocumentsResponse>> PostIndexBereavementDocuments(
       string businessGroup,
       string caseNumber,
       IList<UploadedDocument> documents);

    Task<Either<PostIndexError, PostindexDocumentsResponse>> IndexNonCaseDocuments(
       string businessGroup,
       string referenceNumber,
       IList<UploadedDocument> documents);

    Task<Either<DocumentUploadError, DocumentUploadResponse>> UploadDocumentBase64(
      string businessGroup,
      string fileName,
      string blob,
      int? batchNumber = null);
}