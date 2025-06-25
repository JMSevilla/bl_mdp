using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.EpaService;
using WTW.MdpService.Retirement;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public class CalculationsParser : ICalculationsParser
{
    private readonly CalculationServiceOptions _options;
    private readonly IEpaServiceClient _epaServiceClient;

    public CalculationsParser(IOptionsSnapshot<CalculationServiceOptions> options, IEpaServiceClient epaServiceClient)
    {
        _options = options.Value;
        _epaServiceClient = epaServiceClient;
    }

    public string GetRetirementJson(RetirementResponse retirementResponse, string eventType)
    {
        return JsonSerializer.Serialize(new Domain.Mdp.Calculations.Retirement(retirementResponse, eventType), SerialiationBuilder.Options());
    }

    public (string, string) GetRetirementJsonV2(RetirementResponseV2 retirementResponseV2, string eventType)
    {
        return (JsonSerializer.Serialize(new RetirementV2Mapper().MapToDomain(retirementResponseV2, eventType), SerialiationBuilder.Options()),
                JsonSerializer.Serialize(retirementResponseV2.Results.Mdp, SerialiationBuilder.Options()));
    }

    public string GetRetirementDatesAgesJson(RetirementDatesAgesResponse retirementDatesAgesResponse)
    {
        return JsonSerializer.Serialize(new RetirementDatesAges(retirementDatesAgesResponse), SerialiationBuilder.Options());
    }

    public string GetTransferQuoteJson(TransferResponse transferResponse)
    {
        return JsonSerializer.Serialize(new TransferQuote(transferResponse), SerialiationBuilder.Options());
    }

    public Domain.Mdp.Calculations.Retirement GetRetirement(string retirementJson)
    {
        var dto = JsonSerializer.Deserialize<RetirementDto>(retirementJson, SerialiationBuilder.Options());
        return new Domain.Mdp.Calculations.Retirement(dto);
    }

    public Domain.Mdp.Calculations.RetirementV2 GetRetirementV2(string retirementJsonV2)
    {
        var retirementDto = JsonSerializer.Deserialize<RetirementDtoV2>(retirementJsonV2, SerialiationBuilder.Options());
        return new RetirementV2Mapper().MapFromDto(retirementDto);
    }

    public (Domain.Mdp.Calculations.Retirement retirementV1, Domain.Mdp.Calculations.RetirementV2 retirementV2) GetRetirementV1OrV2(
        string retirementJson,
        string retirementJsonV2)
    {
        if (string.IsNullOrEmpty(retirementJsonV2))
            return (GetRetirement(retirementJson), null);

        return (null, GetRetirementV2(retirementJsonV2));
    }

    public MdpResponseV2 GetQuotesV2(string quotesJsonV2)
    {
        return JsonSerializer.Deserialize<MdpResponseV2>(quotesJsonV2, SerialiationBuilder.Options());
    }

    public RetirementDatesAges GetRetirementDatesAges(string retirementDatesAgesJson)
    {
        var dto = JsonSerializer.Deserialize<RetirementDatesAgesDto>(retirementDatesAgesJson, SerialiationBuilder.Options());
        return new RetirementDatesAges(dto);
    }

    public TransferQuote GetTransferQuote(string transferQuoteJson)
    {
        var dto = JsonSerializer.Deserialize<TransferQuoteDto>(transferQuoteJson, SerialiationBuilder.Options());
        return new TransferQuote(dto);
    }

    public string GetRetirementJsonV2FromRetirementV2(RetirementV2 retirementV2)
    {
        return JsonSerializer.Serialize(retirementV2, SerialiationBuilder.Options());
    }

    public bool IsGuaranteedQuoteEnabled(string bgroup)
    {
        return _options.GuaranteedQuotesEnabledFor.Contains(bgroup);
    }

    public async Task<bool> IsMemberGMPONly(Member member, string userId)
    {
        var gmpOnlyRuleResult = await _epaServiceClient.GetWebRuleResult(member.BusinessGroup, member.ReferenceNumber, userId, "GMONLY", member.SchemeCode, false);
        return ((WebRuleResultResponse)gmpOnlyRuleResult).Result.ToString().Equals("1");
    }

    public async Task<string> GetMemberJuridiction(Member member, string userId)
    {
        var crwndpRuleResult = await _epaServiceClient.GetWebRuleResult(member.BusinessGroup, member.ReferenceNumber, userId, "CRWNDP", member.SchemeCode, false);

        return ((WebRuleResultResponse)crwndpRuleResult).Result;
    }

    public (DateTime, DateTime) EvaluateDateRangeForGMPOrCrownDependencyMember(Member member, bool GMPOnlyMember, string memberJurisdiction, DateTime fromDate, DateTime toDate)
    {
        var dateOfBirth = member.PersonalDetails.DateOfBirth.Value;
        var availableRetirementDateFrom = fromDate;
        var availableRetirementDateTo = toDate;

        if (GMPOnlyMember)
        {
            availableRetirementDateFrom = dateOfBirth.DateTime.AddYears(
            member.PersonalDetails.Gender.Equals(RetirementConstants.Gender.M.ToString())
            ? RetirementConstants.GMPOnlyMaleMemberRetirementAgeInYears
            : RetirementConstants.GMPOnlyFemaleMemberRetirementAgeInYears);
        }
        else
        {
            if (!string.IsNullOrEmpty(memberJurisdiction))
            {
                availableRetirementDateFrom = dateOfBirth.DateTime.AddYears(RetirementConstants.CrownDependencyMemberMinimumPensiontAgeInYears);

                switch (memberJurisdiction)
                {
                    case var _ when memberJurisdiction.Equals(RetirementConstants.MemberJuridiction.JSY.ToString()) || memberJurisdiction.Equals(RetirementConstants.MemberJuridiction.GSY.ToString()):
                        availableRetirementDateTo = dateOfBirth.DateTime.AddYears(RetirementConstants.CrownDependencyGSYOrJSYMemberMaximumPensionAgeInYears);
                        break;
                    case var _ when memberJurisdiction.Equals(RetirementConstants.MemberJuridiction.IOM.ToString()):
                        availableRetirementDateTo = dateOfBirth.DateTime.AddYears(RetirementConstants.CrownDependencyIOMMemberMaximumPensionAgeInYears);
                        break;

                }
            }
        }

        availableRetirementDateFrom = availableRetirementDateFrom > DateTime.UtcNow ? availableRetirementDateFrom : DateTime.UtcNow.AddDays(1);

        return (availableRetirementDateFrom, availableRetirementDateTo);
    }

    public DateTime? GetCalculationFactorDate(string quotesJsonV2)
    {
        return JsonSerializer.Deserialize<MdpResponseV2>(quotesJsonV2, SerialiationBuilder.Options()).CalculationFactorDate;
    }

    public (bool, DateTime?) GetGuaranteedQuoteDetail(RetirementResponseV2 retirementResponseV2)
    {
        try
        {
            DateTime? expiryDate = null;
            if (retirementResponseV2.Results.Quotation.ExpiryDate.HasValue)
            {
                expiryDate = retirementResponseV2.Results.Quotation.ExpiryDate.Value.ToUniversalTime();
            }

            return (retirementResponseV2.Results.Quotation.Guaranteed, expiryDate);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

}