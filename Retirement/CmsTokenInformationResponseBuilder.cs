using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Retirement;

public class CmsTokenInformationResponseBuilder
{
    private readonly CmsTokenInformationResponse _response;

    public CmsTokenInformationResponseBuilder()
    {
        _response = new CmsTokenInformationResponse();
    }

    public CmsTokenInformationResponseBuilder CalculationSuccessful(bool status)
    {
        _response.SystemDate = DateTime.Now;
        _response.IsCalculationSuccessful = status;
        return this;
    }

    public CmsTokenInformationResponseBuilder WithRetirementDatesAges(RetirementDatesAges retirementDatesAges, DateTime retirementDate, string pensionPaymentDay, DateTimeOffset? retirementJourneyExpirationDate)
    {
        _response.NormalRetirementAge = retirementDatesAges.NormalRetirement();
        _response.TargetRetirementAgeIso = retirementDatesAges.TargetRetirementAgeIso;
        _response.TargetRetirementAge = retirementDatesAges.TargetRetirementAgeYearsIso?.ParseIsoDuration()?.Years;
        _response.NormalRetirementDate = retirementDatesAges.NormalRetirementDate;
        _response.TargetRetirementDate = retirementDatesAges.TargetRetirementDate;
        _response.EarliestRetirementAge = retirementDatesAges.EarliestRetirement();
        _response.EarliestRetirementDate = retirementDatesAges.EarliestRetirementDate;
        _response.LatestRetirementAge = retirementDatesAges.GetLatestRetirementAge();
        _response.AgeAtNormalRetirementIso = retirementDatesAges.AgeAtNormalRetirementIso;
        _response.SelectedRetirementDate = retirementDate;
        _response.PensionPaymentDay = pensionPaymentDay;
        _response.RetirementJourneyExpirationDate = retirementJourneyExpirationDate;
        return this;
    }

    public CmsTokenInformationResponseBuilder WithRetirementTime(string timeToTargetRetirementIso, string timeToNormalRetirementIso)
    {
        _response.TimeToTargetRetirementIso = timeToTargetRetirementIso;
        _response.TimeToNormalRetirementIso = timeToNormalRetirementIso;
        return this;
    }

    public CmsTokenInformationResponseBuilder WithRetirementV2Data(
        Option<Domain.Mdp.Calculations.RetirementV2> retirement,
        string selectedQuoteName,
        string businessGroup,
        string schemeType,
        DateTimeOffset? submissionDate = null,
        DateTimeOffset? quoteExpiryDate = null)
    {
        retirement.IfSome(
            r =>
            {
                _response.ChosenLtaPercentage = r.GetTotalLtaUsedPerc(selectedQuoteName, businessGroup, schemeType);
                _response.RemainingLtaPercentage = r.TotalLtaRemainingPercentage;
                _response.LtaLimit = r.StandardLifetimeAllowance;
                _response.GmpAgeYears = r.GMPAgeYears();
                _response.GmpAgeMonths = r.GMPAgeMonths();
                _response.Pre88GMPAtGMPAge = r.Pre88GMPAtGMPAge;
                _response.Post88GMPAtGMPAge = r.Post88GMPAtGMPAge;
                _response.Post88GMPIncreaseCap = r.Post88GMPIncreaseCap;
                _response.StatePensionDeduction = r.StatePensionDeduction;
                _response.NormalMinimumPensionAgeYears = r.NormalMinimumPensionAgeYears();
                _response.NormalMinimumPensionAgeMonths = r.NormalMinimumPensionAgeMonths();
                _response.TotalPension = r.TotalPension();
                _response.TotalAVCFund = r.TotalAVCFund();
                _response.SubmissionDate = submissionDate;
                _response.QuoteExpiryDate = quoteExpiryDate;
            });
        return this;
    }

    public CmsTokenInformationResponseBuilder WithMemberData(Member member, DateTime? retirementDate, DateTimeOffset utcNow)
    {
        _response.SelectedRetirementAge = retirementDate.HasValue ? member.AgeOnSelectedDate(retirementDate.Value) : null;
        _response.Name = member.PersonalDetails.Forenames;
        _response.Email = member.Email().SingleOrDefault();
        _response.PhoneNumber = member.FullMobilePhoneNumber().SingleOrDefault();
        _response.InsuranceNumber = member.InsuranceNumber;
        _response.Address = member.Address().SingleOrDefault();
        _response.DateOfBirth = member.PersonalDetails.DateOfBirth;
        _response.CurrentAgeIso = member.CurrentAgeIso(utcNow);
        _response.AgeAtSelectedRetirementDateIso = retirementDate.HasValue ? member.AgeAtSelectedRetirementDateIso(retirementDate.Value) : null;
        return this;
    }

    public CmsTokenInformationResponseBuilder WithTransferQuoteData(TransferQuote transferQuote)
    {
        _response.TransferReplyByDate = transferQuote?.ReplyByDate;
        _response.TransferGuaranteeExpiryDate = transferQuote?.GuaranteeDate;
        _response.TransferGuaranteePeriodMonths = transferQuote?.GuaranteePeriod.ParseIsoDuration()?.Months;
        _response.TransferQuoteRunDate = transferQuote?.OriginalEffectiveDate;
        return this;
    }

    public CmsTokenInformationResponseBuilder WithDirectRetirementDatesAgesResponseFromApi(Result<RetirementDatesAgesResponse> retirementDatesAgesResponse)
    {
        if (retirementDatesAgesResponse.IsFaulted)
            return this;

        _response.NormalRetirementAge = decimal.ToInt32(retirementDatesAgesResponse.Value().RetirementAges.NormalRetirementAge);
        _response.TargetRetirementAgeIso = retirementDatesAgesResponse.Value().RetirementAges.TargetRetirementAgeIso;
        _response.NormalRetirementDate = retirementDatesAgesResponse.Value().RetirementDates.NormalRetirementDate;
        _response.TargetRetirementDate = retirementDatesAgesResponse.Value().RetirementDates.TargetRetirementDate;
        _response.EarliestRetirementAge = decimal.ToInt32(retirementDatesAgesResponse.Value().RetirementAges.EarliestRetirementAge);
        _response.EarliestRetirementDate = retirementDatesAgesResponse.Value().RetirementDates.EarliestRetirementDate;
        _response.LatestRetirementAge = retirementDatesAgesResponse.Value().RetirementAges.LatestRetirementAge.HasValue ?
            decimal.ToInt32(retirementDatesAgesResponse.Value().RetirementAges.LatestRetirementAge.Value) :
            default;
        _response.AgeAtNormalRetirementIso = retirementDatesAgesResponse.Value().RetirementAges.AgeAtNormalRetirementIso;
        return this;
    }

    public CmsTokenInformationResponse Build()
    {
        return _response;
    }
}
