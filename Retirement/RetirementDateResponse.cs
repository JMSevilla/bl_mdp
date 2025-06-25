using System;
using System.Collections.Generic;

namespace WTW.MdpService.Retirement;

public record RetirementDateResponse
{
    public bool IsCalculationSuccessful { get; init; }
    public DateTimeOffset? RetirementDate { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public DateTimeOffset? QuoteExpiryDate { get; init; }
    public bool? GuaranteedQuote { get; init; }
    public PeriodResponse AvailableRetirementDateRange { get; init; }
    public List<DateTime> GuaranteedQuoteEffectiveDateList { get; init; }

    public static RetirementDateResponse From(
        DateTimeOffset retirementDate,
        DateTimeOffset dateOfBirth,
        DateTimeOffset? quoteExpiryDate,
        bool? guaranteedQuote,
        DateTimeOffset availableRetirementDateFrom,
        DateTimeOffset availableRetirementDateTo,
        List<DateTime> effectiveDateList = null,
        bool isCalculationSuccessful = true
        )
    {
        return new()
        {
            IsCalculationSuccessful = isCalculationSuccessful,
            RetirementDate = retirementDate.Date,
            DateOfBirth = dateOfBirth,
            QuoteExpiryDate = quoteExpiryDate,
            GuaranteedQuote = guaranteedQuote,
            AvailableRetirementDateRange = new PeriodResponse
            {
                From = availableRetirementDateFrom.Date,
                To = availableRetirementDateTo.Date
            },
            GuaranteedQuoteEffectiveDateList = effectiveDateList,
        };
    }

    public record PeriodResponse
    {
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }

    public static RetirementDateResponse CalculationFailed()
    {
        return new()
        {
            IsCalculationSuccessful = false
        };
    }
}