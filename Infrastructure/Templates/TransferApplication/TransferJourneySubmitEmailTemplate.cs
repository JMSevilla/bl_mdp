using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public class TransferJourneySubmitEmailTemplate : ITransferJourneySubmitEmailTemplate
{
    public async Task<string> RenderHtml(string htmlTemplate, Member member)
    {

        return await Template.Parse(htmlTemplate).RenderAsync(
            new
            {
                MemberForenames = member.PersonalDetails.Forenames,
                MemberTitle = member.PersonalDetails.Title,
                MemberSurname = member.PersonalDetails.Surname
            });
    }
}