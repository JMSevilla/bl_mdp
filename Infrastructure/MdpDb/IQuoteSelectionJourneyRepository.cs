using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IQuoteSelectionJourneyRepository
{
    Task Add(QuoteSelectionJourney journey);
    Task<Option<QuoteSelectionJourney>> Find(string businessGroup, string referenceNumber);
    void Remove(QuoteSelectionJourney journey);
}