using WTW.MdpService.Cases;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.Web.Sorting;

namespace WTW.MdpService.Infrastructure.MemberDb.Cases;

public class CasesOrder
{
    public Order<CaseListResponse> Create(string propertyName, bool? ascending)
    {
        return (propertyName, ascending) switch
        {
            ("CaseCode", true) => Order<CaseListResponse>.Create(x => x.CaseCode),
            ("CreationDate", true) => Order<CaseListResponse>.Create(x => x.CreationDate),
            ("CaseStatus", true) => Order<CaseListResponse>.Create(x => x.CaseStatus),
            ("CaseCode", false) => Order<CaseListResponse>.CreateByDescending(x => x.CaseCode),
            ("CreationDate", false) => Order<CaseListResponse>.CreateByDescending(x => x.CreationDate),
            ("CaseStatus", false) => Order<CaseListResponse>.CreateByDescending(x => x.CaseStatus),
            _ => Order<CaseListResponse>.Create(x => x.CaseCode),
        };
    }
}