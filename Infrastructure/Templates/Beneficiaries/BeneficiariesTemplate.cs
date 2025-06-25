using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Members.Beneficiaries;

namespace WTW.MdpService.Infrastructure.Templates.Beneficiaries;

public static class BeneficiariesTemplate
{
    public static async Task<string> RenderHtml(string htmlTemplate, IEnumerable<Beneficiary> beneficiaries)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(
            new
            {
                Beneficiaries = beneficiaries.Select(x => new
                {
                    Relationship = x.BeneficiaryDetails.Relationship,
                    Forenames = x.BeneficiaryDetails.Forenames,
                    Surname = x.BeneficiaryDetails.MixedCaseSurname,
                    DateOfBirth = x.BeneficiaryDetails.DateOfBirth,
                    CharityName = x.BeneficiaryDetails.CharityName,
                    CharityNumber = x.BeneficiaryDetails.CharityNumber,
                    LumpSumPercentage = x.BeneficiaryDetails.LumpSumPercentage,
                    IsPensionBeneficiary = x.IsPensionBeneficiary(),
                    Notes = x.BeneficiaryDetails.Notes,
                }),
                LastUpdateDate = beneficiaries.FirstOrDefault()?.NominationDate
            });
    }
}