using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public class RetirementApplicationSubmissionTemplate : IRetirementApplicationSubmissionTemplate
{
    private readonly IRetirementApplicationQuotesV2 _retirementApplicationQuotes;

    public RetirementApplicationSubmissionTemplate(IRetirementApplicationQuotesV2 retirementApplicationQuotes)
    {
        _retirementApplicationQuotes = retirementApplicationQuotes;
    }

    public async Task<string> Render(string template, string contentAccessKey, CmsTokenInformationResponse cmsToken, RetirementJourney journey, Member member, string contentBlockKeys)
    {
        var bankAccount = member.EffectiveBankAccount();
        var contentKeys = !string.IsNullOrEmpty(contentBlockKeys) ? contentBlockKeys.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
        var templateData = await _retirementApplicationQuotes.Create(journey, contentKeys, contentAccessKey, cmsToken);

        return await Template.Parse(template).RenderAsync(
            new
            {
                MemberQuoteReferenceNumber = journey.ReferenceNumber,
                MemberQuoteStartDate = journey.StartDate.ToString("dd MMMM yyyy"),
                MemberQuoteLabel = journey.MemberQuote.Label,
                MemberQuoteAnnuityPurchaseAmount = journey.MemberQuote.AnnuityPurchaseAmount,
                MemberQuoteLumpSumFromDb = journey.MemberQuote.LumpSumFromDb,
                MemberQuoteLumpSumFromDc = journey.MemberQuote.LumpSumFromDc,
                MemberQuoteMaximumLumpSum = journey.MemberQuote.MaximumLumpSum,
                MemberQuoteMinimumLumpSum = journey.MemberQuote.MinimumLumpSum,
                MemberQuoteSearchedRetirementDate = journey.MemberQuote.SearchedRetirementDate.ToString("dd MMMM yyyy"),
                MemberQuoteNormalRetirementDate = journey.MemberQuote.NormalRetirementDate.ToString("dd MMMM yyyy"),
                MemberQuoteSmallPotLumpSum = journey.MemberQuote.SmallPotLumpSum,
                MemberQuoteTaxFreeUfpls = journey.MemberQuote.TaxFreeUfpls,
                MemberQuoteTaxableUfpls = journey.MemberQuote.TaxableUfpls,
                MemberQuoteTotalLumpSum = journey.MemberQuote.TotalLumpSum,
                MemberQuoteTotalPension = journey.MemberQuote.TotalPension,
                MemberQuoteTotalSpousePension = journey.MemberQuote.TotalSpousePension,
                MemberQuoteTotalUfpls = journey.MemberQuote.TotalUfpls,
                MemberQuoteTransferValueOfDc = journey.MemberQuote.TransferValueOfDc,
                MemberQuoteTrivialCommutationLumpSum = journey.MemberQuote.TrivialCommutationLumpSum,
                MemberQuoteExpirationDate = journey.ExpirationDate.ToString("dd MMMM yyyy"),
                MemberQuoteSubmissionDate = journey.SubmissionDate?.ToString("dd MMMM yyyy"),
                MemberQuoteDateOfLeaving = journey.MemberQuote.DateOfLeaving?.ToString("dd MMMM yyyy"),
                MemberQuoteDatePensionableServiceCommenced = journey.MemberQuote.DatePensionableServiceCommenced,
                MemberQuoteEarliestRetirementAge = journey.MemberQuote.EarliestRetirementAge,
                MemberQuoteFinalPensionableSalary = journey.MemberQuote.FinalPensionableSalary,
                MemberQuoteHasAvcs = journey.MemberQuote.HasAvcs,
                MemberQuoteLtaPercentage = journey.EnteredLtaPercentage,
                MemberQuoteFinancialAdviseDate = journey.FinancialAdviseDate,
                MemberQuotePensionWiseDate = journey.PensionWiseDate,
                MemberQuoteOptOutPensionWise = journey.OptOutPensionWise,
                MemberQuoteNormalRetirementAge = journey.MemberQuote.NormalRetirementAge,
                MemberQuoteSearchedRetirementAge = GetAge(journey.MemberQuote.SearchedRetirementDate, member.PersonalDetails.DateOfBirth.Value),
                MemberQuoteTotalPensionableService = journey.MemberQuote.TotalPensionableService,
                MemberQuoteTransferInService = journey.MemberQuote.TransferInService,
                MemberQuotePensionOptionNumber = journey.MemberQuote.PensionOptionNumber,
                MemberQuoteAcknowledgePensionWise = journey.AcknowledgePensionWise,
                MemberQuoteAcknowledgeFinancialAdvisor = journey.AcknowledgeFinancialAdvisor,
                StatePensionDeduction = journey.MemberQuote.StatePensionDeduction,
                MemberSchemeCode = member.SchemeCode,
                MemberReferenceNumber = member.ReferenceNumber,
                MemberTitle = member.PersonalDetails.Title,
                MemberForenames = member.PersonalDetails.Forenames,
                MemberSurname = member.PersonalDetails.Surname,
                MemberDateOfBirth = member.PersonalDetails.DateOfBirth?.ToString("dd MMMM yyyy"),
                MemberStatus = member.Status.ToString(),
                MemberGender = member.PersonalDetails.Gender,
                MemberCategory = member.Category,
                MemberBankAccountName = bankAccount.MatchUnsafe(a => a.AccountName, () => null),
                MemberBankAccountNumber = bankAccount.MatchUnsafe(a => a.AccountNumber ?? a.Iban, () => null),
                MemberBankSortCode = bankAccount.MatchUnsafe(a => a.Bank?.SortCode, () => null),
                MemberBankAccountIban = bankAccount.MatchUnsafe(a => a.Iban, () => null),
                MemberBankAccountBic = bankAccount.MatchUnsafe(a => a.Bank?.Bic, () => null),
                MemberBankAccountEffectiveDate = bankAccount.MatchUnsafe(a => a.EffectiveDate?.ToString("dd MMMM yyyy"), () => null),
                MemberBankCountryCode = bankAccount.MatchUnsafe(a => a.Bank?.CountryCode, () => null),
                JourneyQuestions = journey.JourneyQuestions().Map(Questions).ToList(),
                MemberQuoteWordingFlags = journey.MemberQuote.ParsedWordingFlags().Map(Flags).ToList(),
                RootQuoteName = journey.MemberQuote.PensionOptionNumber == 0 ? journey.MemberQuote.Label : null,
                Quotev2 = templateData.SelectedOptionData,
                SummaryBlocks = templateData.SummaryBlocks,
                ContentBlocks = templateData.ContentBlockItems,
                CmsTokens = cmsToken,
                GenericDataList = journey.GetJourneyStepsWithGenericData().Select(d => new
                {
                    PageKey = d.CurrentPageKey,
                    Forms = d.JourneyGenericDataList.Select(g => new
                    {
                        g.FormKey,
                        GenericData = g.GenericDataJson
                    })
                }),
            });
    }

    private int GetAge(DateTimeOffset searchedRetirementDate, DateTimeOffset dateOfBirth)
    {
        var age = searchedRetirementDate.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > searchedRetirementDate.AddYears(-age))
            age--;

        return age;
    }

    private JourneyQuestion Questions(QuestionForm form)
    {
        return new()
        {
            QuestionKey = form.QuestionKey,
            AnswerKey = form.AnswerKey
        };
    }

    private MemberQuoteWordingFlag Flags(string flag)
    {
        return new()
        {
            Name = flag,
        };
    }
}