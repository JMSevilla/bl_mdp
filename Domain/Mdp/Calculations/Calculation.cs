using System;
using WTW.MdpService.Retirement;
using WTW.Web;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class Calculation
{
    protected Calculation() { }

    public Calculation(
        string referenceNumber,
        string businessGroup,
        string retirementDatesAgesJson,
        string retirementJson,
        DateTime effectiveRetirementDate,
        DateTimeOffset utcNow,
        bool? isCalculationSuccessful,
        bool? guaranteedQuote,
        DateTime? quoteExpiryDate)
    {
        RetirementDatesAgesJson = retirementDatesAgesJson;
        RetirementJson = retirementJson;
        EffectiveRetirementDate = effectiveRetirementDate;
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        CreatedAt = utcNow;
        IsCalculationSuccessful = isCalculationSuccessful;
        GuaranteedQuote = guaranteedQuote;
        QuoteExpiryDate = quoteExpiryDate;
    }

    public Calculation(
        string referenceNumber,
        string businessGroup,
        string retirementDatesAgesJson,
        string retirementJsonV2,
        string quotesJsonV2,
        DateTime effectiveRetirementDate,
        DateTimeOffset utcNow,
        bool? isCalculationSuccessful)
    {
        RetirementDatesAgesJson = retirementDatesAgesJson;
        RetirementJsonV2 = retirementJsonV2;
        QuotesJsonV2 = quotesJsonV2;
        EffectiveRetirementDate = effectiveRetirementDate;
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        CreatedAt = utcNow;
        IsCalculationSuccessful = isCalculationSuccessful;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public bool? IsCalculationSuccessful { get; private set; }
    public string CalculationStatus { get; private set; }
    public string RetirementDatesAgesJson { get; private set; }
    public string SelectedQuoteName { get; private set; }
    public DateTime EffectiveRetirementDate { get; private set; }
    public string RetirementJson { get; private set; }
    public string RetirementJsonV2 { get; private set; }
    public string QuotesJsonV2 { get; private set; }
    public decimal? EnteredLumpSum { get; private set; }
    public virtual RetirementJourney RetirementJourney { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public bool? GuaranteedQuote { get; private set; }
    public DateTime? QuoteExpiryDate { get; private set; }

    public void UpdateRetirement(string retirementJson, DateTime effectiveRetirementDate, DateTimeOffset utcNow)
    {
        RetirementJson = retirementJson;
        UpdatedAt = utcNow;
        EffectiveRetirementDate = effectiveRetirementDate;
    }

    public void UpdateRetirementJson(string retirementJson)
    {
        RetirementJson = retirementJson;
    }

    public void UpdateRetirementDatesAgesJson(string retirementDatesAgesJson)
    {
        RetirementDatesAgesJson = retirementDatesAgesJson;
    }

    public void UpdateRetirementV2(string retirementJsonV2, string quotesJsonV2, DateTime effectiveRetirementDate, DateTimeOffset utcNow)
    {
        RetirementJsonV2 = retirementJsonV2;
        QuotesJsonV2 = quotesJsonV2;
        UpdatedAt = utcNow;
        EffectiveRetirementDate = effectiveRetirementDate;
    }

    public void UpdateRetirementV2(string retirementJsonV2, string quotesJsonV2, DateTime effectiveRetirementDate, DateTimeOffset utcNow, bool guaranteedQuote, DateTime? quoteExpiryDate = null)
    {
        RetirementJsonV2 = retirementJsonV2;
        QuotesJsonV2 = quotesJsonV2;
        UpdatedAt = utcNow;
        EffectiveRetirementDate = effectiveRetirementDate;
        GuaranteedQuote = guaranteedQuote;
        QuoteExpiryDate = quoteExpiryDate;
    }

    public void UpdateRetirementJsonV2(string retirementJsonV2)
    {
        RetirementJsonV2 = retirementJsonV2;
    }

    public void SetEnteredLumpSum(decimal lumpSum)
    {
        EnteredLumpSum = lumpSum;
    }

    public void ClearLumpSum()
    {
        EnteredLumpSum = null;
    }

    public void SetJourney(RetirementJourney journey, string selectedQuoteName)
    {
        RetirementJourney = journey;
        SelectedQuoteName = selectedQuoteName;
    }

    public (DateTime? EarliestStartRaDateForSelectedDate, DateTime LatestStartRaDateForSelectedDate) RetirementApplicationStartDateRange(DateTimeOffset utcNow)
    {
        if (EffectiveRetirementDate > utcNow.AddMonths(RetirementConstants.RetirementApplicationPeriodInMonths).Date)
        {
            return (
                EffectiveRetirementDate.AddMonths(-RetirementConstants.RetirementApplicationPeriodInMonths),
                EffectiveRetirementDate.AddDays(-RetirementConstants.RetirementProcessingPeriodInDays)
                );
        }

        return (null, EffectiveRetirementDate.AddDays(-RetirementConstants.RetirementProcessingPeriodInDays));
    }

    public DateTime RetirementConfirmationDate()
    {
        return EffectiveRetirementDate.AddDays(-RetirementConstants.RetirementConfirmationInDays);
    }

    public DateTime RetirementDateWithAppliedRetirementProcessingPeriod(int retirementProcessingPeriodInDays, string schemeType, DateTimeOffset utcNow)
    {
        if (schemeType == MdpConstants.SchemeTypeDc || (BusinessGroup == MdpConstants.NatwestBgroup && RetirementJourney == null))
            return EffectiveRetirementDate;

        var earliestEligibleRetirementDate = utcNow.Date.AddDays(retirementProcessingPeriodInDays);
        if ((RetirementJourney == null || RetirementJourney.HasRetirementJourneyExpired(utcNow)) && EffectiveRetirementDate < earliestEligibleRetirementDate)
            return earliestEligibleRetirementDate;

        return EffectiveRetirementDate;
    }

    public void UpdateEffectiveDate(DateTime effectiveRetirementDate)
    {
        EffectiveRetirementDate = effectiveRetirementDate;
    }

    public bool HasRetirementJourneyStarted()
    {
        return RetirementJourney != null;
    }

    public bool IsRetirementJourneySubmitted()
    {
        return RetirementJourney != null && RetirementJourney.IsRetirementJourneySubmitted();
    }

    public bool HasRetirementJourneyExpired(DateTimeOffset utcNow)
    {
        return RetirementJourney != null && RetirementJourney.HasRetirementJourneyExpired(utcNow);
    }

    public DateTimeOffset ExpectedRetirementJourneyExpirationDate(DateTimeOffset utcNow, int daysToExpire)
    {
        return RetirementJourney != null && !RetirementJourney.HasRetirementJourneyExpired(utcNow)
            ? RetirementJourney.ExpirationDate
            : RetirementJourney.CalculateExpireDate(EffectiveRetirementDate.ToUniversalTime().Date, utcNow, RetirementConstants.RetirementProcessingPeriodInDays, daysToExpire);
    }

    public void SetCalculationSuccessStatus(bool? status)
    {
        if (IsCalculationSuccessful == null)
            IsCalculationSuccessful = status;
    }

    public void UpdateCalculationSuccessStatus(bool status)
    {
        IsCalculationSuccessful = status;
    }

    public Calculation SetCalculationStatus(string status)
    {
        CalculationStatus = status;
        return this;
    }

    public bool IsRetirementDateOutOfRange()
    {
        return EffectiveRetirementDate.Date > DateTimeOffset.UtcNow.Date.AddMonths(RetirementConstants.RetirementApplicationPeriodInMonths);
    }
}