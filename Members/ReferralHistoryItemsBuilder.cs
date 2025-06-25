using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public class ReferralHistoryItemsBuilder
{
    private readonly List<IfaReferralHistory> _referralHistory;
    private const int AVAILABLE_STATUS_COUNT = 8;
    private const string CANCELLED_REFERRAL_VALUE = "3";


    public ReferralHistoryItemsBuilder(IEnumerable<IfaReferralHistory> referralHistory)
    {
        _referralHistory = referralHistory.OrderBy(x => x.SequenceNumber).ToList();
    }

    public IEnumerable<ReferralHistoryItem> ReferralHistoryItems()
    {
        if(_referralHistory.Count == 0)
            return new List<ReferralHistoryItem>{new()
            {
                ReferralStatus = _referralStatusDisplayNameMap[ReferralStatus.ReferralInitiated],
                ReferralBadgeStatus = ReferralBadgeStatus.Pending
            }};
        
        var fullHistory = new List<ReferralHistoryItem>();

        int gaps = 1; // since index starts on 0 and status on 1 we have to add 1 to the gaps
        for (int i = 0; i < _referralHistory.Count; i++)
        {
            if ((int)_referralHistory[i].ReferralStatus != i + gaps)
            {
                for (int j =  i + gaps; j < (int)_referralHistory[i].ReferralStatus; j++)
                {
                    fullHistory.Add(new ReferralHistoryItem
                    {
                        ReferralDate = ReferralDate(_referralHistory, i),
                        ReferralStatus = _referralStatusDisplayNameMap[(ReferralStatus)j],
                        ReferralBadgeStatus = ReferralBadgeStatus.Completed
                    });
                }
                gaps = (int)_referralHistory[i].ReferralStatus - i;
            }

            fullHistory.Add(new ReferralHistoryItem
            {
                ReferralDate = ReferralDate(_referralHistory, i),
                ReferralStatus = _referralStatusDisplayNameMap[_referralHistory[i].ReferralStatus],
                ReferralBadgeStatus = ReferralBadgeStatus.Completed
            });
        }

        if (fullHistory.Count < AVAILABLE_STATUS_COUNT)
            fullHistory.Add(new ReferralHistoryItem {
                ReferralDate = LastReferralDate(_referralHistory),
                ReferralStatus = _referralStatusDisplayNameMap[_referralHistory[^1].ReferralStatus + 1],
                ReferralBadgeStatus = LastBadgeStatus(_referralHistory[_referralHistory.Length() - 1].ReferralResult)
            });

        return fullHistory;
    }
    
    private DateTimeOffset? ReferralDate(List<IfaReferralHistory> referralHistory, int index)
    {
        if (index == 0)
            return referralHistory[index].ReferralInitiatedOn;
        return referralHistory[index].ReferralStatusDate;
    }
    
    private DateTimeOffset? LastReferralDate(List<IfaReferralHistory> referralHistory)
    {
        if(LastBadgeStatus(_referralHistory[_referralHistory.Length() - 1].ReferralResult) == ReferralBadgeStatus.Pending)
            return null;

        return ReferralDate(referralHistory, _referralHistory.Length() - 1);
    }
    
    private ReferralBadgeStatus LastBadgeStatus(string result)
    {
        if (result == CANCELLED_REFERRAL_VALUE)
            return ReferralBadgeStatus.Cancelled;

        return ReferralBadgeStatus.Pending;
    }
    
    private static readonly IDictionary<ReferralStatus, string> _referralStatusDisplayNameMap =
        new Dictionary<ReferralStatus, string>
        {
            {ReferralStatus.ReferralInitiated, "REFERRAL_INITIATED"},
            {ReferralStatus.WelcomePack, "WELCOME_PACK"},
            {ReferralStatus.WelcomePackIssued, "WELCOME_PACK_ISSUED"},
            {ReferralStatus.FirstAppointmentArranged, "FIRST_APPOINTMENT"},
            {ReferralStatus.FactFind, "FACT_FIND"},
            {ReferralStatus.FactFindStarted, "FACT_FIND_STARTED"},
            {ReferralStatus.FactFindCompleted, "FACT_FIND_COMPLETED"},
            {ReferralStatus.Recommendation, "RECOMMENDATION"},
        };
}

public enum ReferralBadgeStatus
{
    Completed = 1,
    Pending = 2,
    Cancelled = 3
}