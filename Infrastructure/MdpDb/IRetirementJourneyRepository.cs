using System;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IRetirementJourneyRepository
{
    Task Create(RetirementJourney journey);
    Task<Option<RetirementJourney>> Find(string businessGroup, string referenceNumber);
    Task<Option<RetirementJourney>> FindExpiredJourney(string businessGroup, string referenceNumber);
    Task<Option<RetirementJourney>> FindUnexpiredJourney(string businessGroup, string referenceNumber, DateTimeOffset now);
    Task<Option<RetirementJourney>> FindUnexpiredOrSubmittedJourney(string businessGroup, string referenceNumber, DateTimeOffset now);
    Task<Option<RetirementJourney>> FindUnexpiredUnsubmittedJourney(string businessGroup, string referenceNumber, DateTimeOffset now);
    void Remove(RetirementJourney journey);
}