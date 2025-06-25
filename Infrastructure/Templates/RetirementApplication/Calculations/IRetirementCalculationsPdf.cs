using System.IO;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations
{
    public interface IRetirementCalculationsPdf
    {
        Task<MemoryStream> GenerateOptionsPdf(string contentAccessKey, Calculation calculation, Member member, string businessGroup, string accessToken, string env);
        Task<MemoryStream> GenerateSummaryPdf(string contentAccessKey, Calculation calculation, Member member, string summaryKey, string businessGroup, string accessToken, string env);
    }
}