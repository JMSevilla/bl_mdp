using System;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.Ifa;

public static class IfaCompanyTemplate
{
    public static async Task<string> Render(string template, string referenceNumber, string email, PersonalDetails personalDetails, string number, DateTimeOffset? originalEffectiveDate)
    {
        return await Template.Parse(template).RenderAsync(
            new
            {
                MemberForenames = personalDetails.Forenames,
                MemberSurname = personalDetails.Surname,
                MemberRefnumber = referenceNumber,
                MemberQuoteSearchedRetirementDate = originalEffectiveDate,
                MemberPhoneNumber = number,
                MemberEmail = email
            });
    }
}