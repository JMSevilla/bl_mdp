using System;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Journeys;

public class JourneyService : IJourneyService
{
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly IBereavementJourneyRepository _bereavementJourneyRepository;
    private readonly IJourneysRepository _journeysRepository;

    public JourneyService(
        IRetirementJourneyRepository retirementJourneyRepository,
        ITransferJourneyRepository transferJourneyRepository,
        IBereavementJourneyRepository bereavementJourneyRepository,
        IJourneysRepository journeysRepository)
    {
        _retirementJourneyRepository = retirementJourneyRepository;
        _transferJourneyRepository = transferJourneyRepository;
        _bereavementJourneyRepository = bereavementJourneyRepository;
        _journeysRepository = journeysRepository;
    }

    public async Task<Option<Journey>> GetJourney(string journeyType, string businessGroup, string referenceNumber)
    {
        switch (journeyType.ToLower())
        {
            case "retirement":
            case "dbretirementapplication":
                var retirementJourney = await _retirementJourneyRepository.FindUnexpiredUnsubmittedJourney(businessGroup, referenceNumber, DateTimeOffset.UtcNow);
                if (retirementJourney.IsSome)
                    return retirementJourney.Value();
                break;
            case "transfer2":
                var transferJourney = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
                if (transferJourney.IsSome)
                    return transferJourney.Value();
                break;
            case "bereavement":
                var bereavementJourney = await _bereavementJourneyRepository.FindUnexpired(businessGroup, Guid.Parse(referenceNumber), DateTimeOffset.UtcNow);
                if (bereavementJourney.IsSome)
                    return bereavementJourney.Value();
                break;
        };

        var genericJourney = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (genericJourney.IsSome)
            return genericJourney.Value();

        return Option<Journey>.None;
    }


    public async Task<Option<RetirementJourney>> FindUnexpiredOrSubmittedJourney(string businessGroup, string referenceNumber)
    {
        var retirementJourney = await _retirementJourneyRepository.FindUnexpiredOrSubmittedJourney(businessGroup, referenceNumber, DateTimeOffset.UtcNow);
        if (retirementJourney.IsSome)
            return retirementJourney.Value();

        return Option<RetirementJourney>.None;
    }

}