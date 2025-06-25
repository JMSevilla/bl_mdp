using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Retirement;
using Calculation = WTW.MdpService.Domain.Mdp.Calculations.Calculation;

namespace WTW.MdpService.Domain.Mdp;

public class RetirementJourney : Journey
{
    protected RetirementJourney() { }

    public RetirementJourney(string businessGroup,
        string referenceNumber,
        DateTimeOffset utcNow,
        string currentPageKey,
        string nextPageKey,
        MemberQuote memberQuote,
        int daysToExpire,
        int processingInDays,
        DateTimeOffset? submissionDate = null) : base(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey)
    {
        MemberQuote = memberQuote;
        ExpirationDate = CalculateExpireDate(memberQuote.SearchedRetirementDate.Date, utcNow, processingInDays, daysToExpire);
        SubmissionDate = submissionDate;
    }

    public bool? AcknowledgePensionWise { get; private set; }
    public bool? AcknowledgeFinancialAdvisor { get; private set; }
    public bool? OptOutPensionWise { get; private set; }
    public virtual MemberQuote MemberQuote { get; }
    public byte[] SummaryPdf { get; private set; }
    public Guid? GbgId { get; private set; }
    public string CaseNumber { get; private set; }
    public decimal? EnteredLtaPercentage { get; private set; }
    public DateTimeOffset? FinancialAdviseDate { get; private set; }
    public DateTimeOffset? PensionWiseDate { get; private set; }
    public virtual Calculation Calculation { get; private set; }

    public RetirementJourney SetCalculation(Calculation calculation)
    {
        Calculation = calculation;
        return this;
    }

    public Error? SetPensionWiseDate(DateTimeOffset pensionWiseDate)
    {
        if (!DateIsLessThanOrEqualToTodayValidation(pensionWiseDate))
            return Error.New("Pension wise date should be less than or equal to today.");

        PensionWiseDate = pensionWiseDate;
        return null;
    }

    public Error? SetFinancialAdviseDate(DateTimeOffset financialAdviseDate)
    {
        if (!DateIsLessThanOrEqualToTodayValidation(financialAdviseDate))
            return Error.New("Financial advise date should be less than or equal to today.");

        FinancialAdviseDate = financialAdviseDate;
        return null;
    }

    public void SetOptOutPensionWise(bool optOutPensionWise)
    {
        OptOutPensionWise = optOutPensionWise;
    }

    public Either<Error, bool> SetEnteredLtaPercentage(decimal percentage)
    {
        if (LtaPercentageValidation(percentage))
            return Error.New("Percentage should be between 1 and 1000 and should have only 2 digits after dot.");

        EnteredLtaPercentage = percentage;
        return true;
    }

    public IEnumerable<QuestionForm> JourneyQuestions()
    {
        return JourneyBranches.Single(x => x.IsActive).QuestionForms();
    }

    public bool IsRetirementJourneySubmitted()
    {
        return SubmissionDate.HasValue;
    }

    public bool HasRetirementJourneyExpired(DateTimeOffset now)
    {
        return ExpirationDate.Date <= now.Date;
    }

    // TODO: consider other EnteredLtaPercentage uses, maybe it should be made private and this one use instead
    public decimal? ActiveLtaPercentage()
    {
        return ActiveBranch().HasLifetimeAllowance() ? EnteredLtaPercentage : null;
    }

    public void SetFlags(
        bool acknowledgementFinancialAdvisor,
        bool acknowledgementPensionWise)
    {
        AcknowledgeFinancialAdvisor = acknowledgementFinancialAdvisor;
        AcknowledgePensionWise = acknowledgementPensionWise;
    }

    public void Submit(
        byte[] summaryPdf,
        DateTimeOffset now,
        string caseNumber)
    {
        SummaryPdf = summaryPdf;
        SubmissionDate = now;
        CaseNumber = caseNumber;
    }

    public void SaveGbgId(Guid id)
    {
        GbgId = id;
    }

    public RetirementApplicationStatus Status(DateTimeOffset utcNow)
    {
        if (!SubmissionDate.HasValue && ExpirationDate.Date < utcNow.Date)
            return RetirementApplicationStatus.ExpiredRA;

        return SubmissionDate.HasValue ? RetirementApplicationStatus.SubmittedRA : RetirementApplicationStatus.StartedRA;
    }

    public static DateTimeOffset CalculateExpireDate(DateTime retirementDate, DateTimeOffset utcNow, int processingInDays, int daysToExpire)
    {
        var latestExpireDate = utcNow.AddDays(daysToExpire).Date;
        var latestExpireDateForRetirementDate = retirementDate.AddDays(-(processingInDays - RetirementConstants.RetirementSubmissionMinimumWindowInDays));

        if (latestExpireDateForRetirementDate.Date >= latestExpireDate.Date)
            return new DateTimeOffset(latestExpireDate, TimeSpan.FromHours(0));

        if (latestExpireDateForRetirementDate <= utcNow)
            return new DateTimeOffset(utcNow.Date.AddDays(RetirementConstants.RetirementSubmissionMinimumWindowInDays), TimeSpan.FromHours(0));

        return new DateTimeOffset(latestExpireDateForRetirementDate.Date, TimeSpan.FromHours(0));
    }

    public bool IsGbgStepOlderThan30Days(DateTimeOffset utcNow)
    {
        return ActiveBranch().JourneySteps.SingleOrDefault(s => s.CurrentPageKey == "submit_document_info")?.SubmitDate.AddDays(30) < utcNow;
    }

    private static readonly Func<decimal, bool> LtaPercentageValidation = percentage =>
    {
        return percentage > 999.99m || percentage < 0.01m || decimal.Round(percentage, 2) != percentage;
    };

    private static readonly Func<DateTimeOffset, bool> DateIsLessThanOrEqualToTodayValidation = date =>
    {
        return date.Date <= DateTimeOffset.Now.Date;
    };

    private JourneyBranch ActiveBranch()
    {
        return JourneyBranches.Single(x => x.IsActive);
    }
}