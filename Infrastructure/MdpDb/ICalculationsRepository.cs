using System;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface ICalculationsRepository
{
    Task Create(Calculation calculation);
    Task<bool> ExistsWithUnexpiredRetirementJourney(string referenceNumber, string businessGroup, DateTimeOffset now);
    Task<Option<Calculation>> Find(string referenceNumber, string businessGroup);
    Task<Option<Calculation>> FindWithJourney(string referenceNumber, string businessGroup);
    Task<Option<Calculation>> FindWithValidRetirementJourney(string referenceNumber, string businessGroup, DateTimeOffset now);
    void Remove(Calculation calculation);
}