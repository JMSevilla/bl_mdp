using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp.Calculations
{
    public interface IRetirementService
    {
        Dictionary<string, object> GetSelectedQuoteDetails(string name, RetirementV2 retirement);
    }
}