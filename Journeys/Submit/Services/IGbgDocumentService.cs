using System;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface IGbgDocumentService
{
    Task<Either<Error, (MemoryStream DocumentStream, string FileName)>> GetGbgFile(string businessGroup, string referenceNumber, string journeyType, string caseNumber);
    Task<Either<Error, (MemoryStream DocumentStream, string FileName)>> GetGbgFile(Guid gbgId);
}