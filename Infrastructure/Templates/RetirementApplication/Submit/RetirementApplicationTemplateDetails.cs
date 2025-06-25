using System;
using System.Collections.Generic;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public class RetirementApplicationTemplateDetails
{
    public string MemberQuoteReferenceNumber { get; set; }
    public DateTimeOffset? MemberQuoteStartDate { get; set; }
    public string MemberQuoteLabel { get; set; }
    public string RootQuoteName { get; set; }
    public decimal? MemberQuoteAnnuityPurchaseAmount { get; set; }
    public decimal? MemberQuoteLumpSumFromDb { get; set; }
    public decimal? MemberQuoteLumpSumFromDc { get; set; }
    public decimal? MemberQuoteMaximumLumpSum { get; set; }
    public decimal? MemberQuoteMinimumLumpSum { get; set; }
    public DateTimeOffset? MemberQuoteSearchedRetirementDate { get; set; }
    public DateTimeOffset? MemberQuoteNormalRetirementDate { get; set; }
    public decimal? MemberQuoteSmallPotLumpSum { get; set; }
    public decimal? MemberQuoteTaxFreeUfpls { get; set; }
    public decimal? MemberQuoteTaxableUfpls { get; set; }
    public decimal? MemberQuoteTotalLumpSum { get; set; }
    public decimal? MemberQuoteTotalPension { get; set; }
    public decimal? MemberQuoteTotalSpousePension { get; set; }
    public decimal? MemberQuoteTotalUfpls { get; set; }
    public decimal? MemberQuoteTransferValueOfDc { get; set; }
    public decimal? MemberQuoteTrivialCommutationLumpSum { get; set; }
    public DateTimeOffset? MemberQuoteExpirationDate { get; set; }
    public DateTimeOffset? MemberQuoteSubmissionDate { get; set; }
    public DateTimeOffset? MemberQuoteDateOfLeaving { get; set; }
    public DateTimeOffset? MemberQuoteDatePensionableServiceCommenced { get; set; }
    public int? MemberQuoteEarliestRetirementAge { get; set; }
    public decimal? MemberQuoteFinalPensionableSalary { get; set; }
    public bool? MemberQuoteHasAvcs { get; set; }
    public decimal? MemberQuoteLtaPercentage { get; set; }
    public DateTimeOffset? MemberQuoteFinancialAdviseDate { get; set; }
    public DateTimeOffset? MemberQuotePensionWiseDate { get; set; }
    public bool? MemberQuoteOptOutPensionWise { get; set; }
    public int? MemberQuoteNormalRetirementAge { get; set; }
    public int? MemberQuoteSearchedRetirementAge { get; set; }
    public string MemberQuoteTotalPensionableService { get; set; }
    public string MemberQuoteTransferInService { get; set; }
    public string MemberQuoteCalculationType { get; set; }
    public int? MemberQuotePensionOptionNumber { get; set; }
    public bool? MemberQuoteAcknowledgePensionWise { get; set; }
    public bool? MemberQuoteAcknowledgeFinancialAdvisor { get; set; }
    public decimal? MemberQuoteStatePensionDeduction { get; set; }
    public string MemberSchemeCode { get; set; }
    public string MemberReferenceNumber { get; set; }
    public string MemberTitle { get; set; }
    public string MemberForenames { get; set; }
    public string MemberSurname { get; set; }
    public DateTimeOffset? MemberDateOfBirth { get; set; }
    public DateTimeOffset? MemberDateJoinSheme { get; set; }
    public DateTimeOffset? MemberDateLeftSheme { get; set; }
    public string MemberStatus { get; set; }
    public string MemberGender { get; set; }
    public string MemberCategory { get; set; }
    public string MemberNiNumber { get; set; }
    public DateTimeOffset? MemberRetirementDate { get; set; }
    public DateTimeOffset? MemberOverrideRetirementDate { get; set; }
    public int? MemberOverrideMinimumPensionAge { get; set; }
    public decimal? MemberLtaPercentage { get; set; }
    public string MemberBankAccountName { get; set; }
    public string MemberBankAccountNumber { get; set; }
    public string MemberBankSortCode { get; set; }
    public string MemberBankAccountIban { get; set; }
    public string MemberBankAccountBic { get; set; }
    public DateTimeOffset? MemberBankAccountEffectiveDate { get; set; }
    public string MemberBankCountryCode { get; set; }
    public IEnumerable<JourneyQuestion> JourneyQuestions { get; set; }
    public IEnumerable<MemberQuoteWordingFlag> MemberQuoteWordingFlags { get; set; }
}
