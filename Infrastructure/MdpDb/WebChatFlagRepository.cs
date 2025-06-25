using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Oracle.ManagedDataAccess.Client;
using WTW.MdpService.Infrastructure.MemberDb;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class WebChatFlagRepository : IWebChatFlagRepository
{
    private readonly MemberDbContext _context;

    public WebChatFlagRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<string> CheckBgroup(string bGroup)
    {
        var domainList = await _context.DomainLists.FirstOrDefaultAsync(x =>
                                              x.BusinessGroup == bGroup && 
                                              x.Domain == "WBCHT" &&
                                              x.ListOfValidValues == "Y");

        if (domainList == default)
            return string.Empty;

        return domainList.ListOfValidValues;
    }


    public async Task<string> CheckMemberCriteria(string bGroup, string schemeCode, string statusCode)
    {
        var returnValue = "1";

        var contactCentreRule = await _context.ContactCentreRules.FirstOrDefaultAsync(x =>
                                                x.BusinessGroup == bGroup &&
                                                x.WebchatFlag == "Y" &&
                                                (
                                                    (x.Scheme == schemeCode && x.MemberStatus == statusCode) ||
                                                    (x.Scheme == "*" && x.MemberStatus == statusCode) ||
                                                    (x.Scheme == schemeCode && x.MemberStatus == "*") ||
                                                    (x.Scheme == "*" && x.MemberStatus == "*")
                                                ));
        if (contactCentreRule == default)
            return string.Empty;

        return returnValue;
    }
}
