using WTW.MdpService.Domain.Members;
using WTW.Web.Sorting;

namespace WTW.MdpService.Infrastructure.MemberDb.Documents;

public static class DocumentsOrder
{
    public static Order<Document> Create(string propertyName, bool ascending)
    {
        return (propertyName, ascending) switch
        {
            ("Name", true) => Order<Document>.Create(x => x.Date),
            ("DateReceived", true) => Order<Document>.Create(x => x.Date),
            ("Type", true) => Order<Document>.Create(x => x.Type),
            ("Name", false) => Order<Document>.CreateByDescending(x => x.Name),
            ("DateReceived", false) => Order<Document>.CreateByDescending(x => x.Date),
            ("Type", false) => Order<Document>.CreateByDescending(x => x.Type),
            _ => Order<Document>.Create(x => x.Name),
        };
    }
}