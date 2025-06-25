using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;
using WTW.MdpService.Journeys.JourneysGenericData;

namespace WTW.MdpService.RetirementJourneys;

public record RetirementApplicationResponse
{
    public string Label { get; init; }
    public DateTimeOffset LastSearchedRetirementDate { get; init; }
    public decimal? AnnuityPurchaseAmount { get; init; }
    public decimal? LumpSumFromDb { get; init; }
    public decimal? LumpSumFromDc { get; init; }
    public decimal? SmallPotLumpSum { get; init; }
    public decimal? TaxFreeUfpls { get; init; }
    public decimal? TaxableUfpls { get; init; }
    public decimal? TotalLumpSum { get; init; }
    public decimal? TotalPension { get; init; }
    public decimal? TotalSpousePension { get; init; }
    public decimal? TotalUfpls { get; init; }
    public decimal? TransferValueOfDc { get; init; }
    public decimal? MinimumLumpSum { get; init; }
    public decimal? MaximumLumpSum { get; init; }
    public decimal? TrivialCommutationLumpSum { get; init; }
    public DateTimeOffset ExpirationDate { get; init; }
    public DateTimeOffset SelectedRetirementDate { get; init; }
    public RetirementApplicationStatus RetirementApplicationStatus { get; init; }
    public DateTimeOffset? SubmissionDate { get; init; }
    public bool HasAvcs { get; init; }
    public List<SummaryFigure> SummaryFigures { get; init; } = new();
    public IEnumerable<JourneyGenericDataResponse> JourneyGenericDataList { get; init; }

    public static RetirementApplicationResponse From(MemberQuote memberQuote,
        DateTimeOffset expirationDate, DateTimeOffset selectedRetirementDate,
        DateTimeOffset? submissionDate, RetirementApplicationStatus retirementApplicationStatus,
        RetirementSummary retirementSummaryItems,
        IEnumerable<JourneyGenericData> journeyGenericDataList)
    {
        return new()
        {
            Label = memberQuote.Label,
            LastSearchedRetirementDate = memberQuote.SearchedRetirementDate,
            AnnuityPurchaseAmount = memberQuote.AnnuityPurchaseAmount,
            LumpSumFromDc = memberQuote.LumpSumFromDc,
            LumpSumFromDb = memberQuote.LumpSumFromDb,
            MaximumLumpSum = memberQuote.MaximumLumpSum,
            SmallPotLumpSum = memberQuote.SmallPotLumpSum,
            MinimumLumpSum = memberQuote.MinimumLumpSum,
            TaxFreeUfpls = memberQuote.TaxFreeUfpls,
            TaxableUfpls = memberQuote.TaxableUfpls,
            TotalPension = memberQuote.TotalPension,
            TotalLumpSum = memberQuote.TotalLumpSum,
            TotalSpousePension = memberQuote.TotalSpousePension,
            TotalUfpls = memberQuote.TotalUfpls,
            TransferValueOfDc = memberQuote.TransferValueOfDc,
            TrivialCommutationLumpSum = memberQuote.TrivialCommutationLumpSum,
            ExpirationDate = expirationDate,
            SelectedRetirementDate = selectedRetirementDate,
            RetirementApplicationStatus = retirementApplicationStatus,
            SubmissionDate = submissionDate,
            HasAvcs = memberQuote.HasAvcs,
            SummaryFigures = retirementSummaryItems.SummaryFigures,
            JourneyGenericDataList = journeyGenericDataList.Select(x => new JourneyGenericDataResponse(x))
        };
    }
}