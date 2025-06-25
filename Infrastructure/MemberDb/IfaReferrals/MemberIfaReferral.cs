using System;
using System.Threading.Tasks;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.MemberDb.IfaReferrals;

public class MemberIfaReferral : IMemberIfaReferral
{
    private readonly IIfaReferralRepository _ifaReferralRepository;
    private readonly ITransferJourneyRepository _transferJourneyRepository;

    public MemberIfaReferral(
        IIfaReferralRepository ifaReferralRepository,
        ITransferJourneyRepository transferJourneyRepository)
    {
        _ifaReferralRepository = ifaReferralRepository;
        _transferJourneyRepository = transferJourneyRepository;
    }

    public async Task<bool> HasIfaReferral(string referenceNumber, string businessGroup, DateTimeOffset now)
    {
        var transfer = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (transfer.IsNone)
            return false;

        var ifa = await _ifaReferralRepository.Find(referenceNumber, businessGroup, transfer.Value().CalculationType);
        if (ifa.IsNone)
            return false;

        return transfer.Value().CalculationType switch
        {
            "PO" => now < ifa.Value().ReferralInitiatedOn.AddMonths(3),
            "AO" => false,
            _ => false
        };
    }

    public async Task WaitForIfaReferral(string referenceNumber, string businessGroup, string calculationType, DateTimeOffset now)
    {
        bool hasIfaReferral;
        var iterator = 0;
        do
        {
            iterator++;
            hasIfaReferral = (await _ifaReferralRepository.Find(referenceNumber, businessGroup, calculationType)).IsSome;
            if (!hasIfaReferral)
                await Task.Delay(2000);

        } while (!hasIfaReferral && iterator < 30);
    }
}