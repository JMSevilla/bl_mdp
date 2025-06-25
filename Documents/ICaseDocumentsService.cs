using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Documents;

public interface ICaseDocumentsService
{
    public Task<Either<Error, string>> GetCaseNumber(string businessGroup, string referenceNumber, string caseCode);
}
