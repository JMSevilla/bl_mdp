using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.BereavementJourneys;

public interface IBereavementCase
{
    Task<Either<Error, string>> Create(string businessGroup, string name, string surname, DateTime? dateOfBirth, DateTime? dateOfDeath, IEnumerable<string> refNumbers);
}