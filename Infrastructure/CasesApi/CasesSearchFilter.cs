using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WTW.MdpService.Cases;
using WTW.Web.Specs;

namespace WTW.MdpService.Infrastructure.CasesApi;

public static class CasesSearchFilter
{
    public static IEnumerable<CaseListResponse> Filter(IEnumerable<CaseListResponse> caseList, IEnumerable<string> caseCodes)
    {
        if (caseCodes == null || !caseCodes.Any())
            return caseList;

        return caseList.Where(x => caseCodes.Contains(x.CaseCode));
    }
}
