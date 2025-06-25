using System;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Calculations;

public interface ICalculationsParser
{
    MdpResponseV2 GetQuotesV2(string quotesJsonV2);
    Domain.Mdp.Calculations.Retirement GetRetirement(string retirementJson);
    RetirementDatesAges GetRetirementDatesAges(string retirementDatesAgesJson);
    string GetRetirementDatesAgesJson(RetirementDatesAgesResponse retirementDatesAgesResponse);
    string GetRetirementJson(RetirementResponse retirementResponse, string eventType);
    (string, string) GetRetirementJsonV2(RetirementResponseV2 retirementResponseV2, string eventType);
    string GetRetirementJsonV2FromRetirementV2(RetirementV2 retirementV2);
    (Domain.Mdp.Calculations.Retirement retirementV1, RetirementV2 retirementV2) GetRetirementV1OrV2(string retirementJson, string retirementJsonV2);
    RetirementV2 GetRetirementV2(string retirementJsonV2);
    TransferQuote GetTransferQuote(string transferQuoteJson);
    string GetTransferQuoteJson(TransferResponse transferResponse);
    bool IsGuaranteedQuoteEnabled(string bgroup);
    DateTime? GetCalculationFactorDate(string quotesJsonV2);
    (bool, DateTime?) GetGuaranteedQuoteDetail(RetirementResponseV2 retirementResponseV2);

    Task<bool> IsMemberGMPONly(Member member, string userId);
    Task<string> GetMemberJuridiction(Member member, string userId);
    (DateTime, DateTime) EvaluateDateRangeForGMPOrCrownDependencyMember(Member member, bool GMPOnlyMember, string memberJuridiction, DateTime fromDate, DateTime toDate);
}