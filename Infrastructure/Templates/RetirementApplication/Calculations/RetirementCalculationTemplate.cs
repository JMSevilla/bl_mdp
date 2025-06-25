using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Retirement;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;

public class RetirementCalculationTemplate : IRetirementApplicationCalculationTemplate
{
    private readonly IRetirementCalculationQuotesV2 _retirementCalculationQuotes;

    public RetirementCalculationTemplate(IRetirementCalculationQuotesV2 retirementCalculationQuotes)
    {
        _retirementCalculationQuotes = retirementCalculationQuotes;
    }

    public async Task<string> Render(string template, string selectedQuoteName, JsonElement summary, CmsTokenInformationResponse cmsToken, Calculation calculation, Member member, IEnumerable<JsonElement> contentBlocks, (string AccessToken, string Env, string Bgroup) auth)
    {
        var bankAccount = member.EffectiveBankAccount();
        var (quotesV2, summaryBlocks) = await _retirementCalculationQuotes.Create(calculation, selectedQuoteName, summary, auth);
        var contentBlockItems = _retirementCalculationQuotes.GetContentBlocks(contentBlocks, cmsToken);

        return await Template.Parse(template).RenderAsync(
            new
            {
                MemberQuoteReferenceNumber = calculation.ReferenceNumber,
                MemberQuoteLabel = selectedQuoteName,
                MemberQuoteSearchedRetirementDate = calculation.EffectiveRetirementDate.ToString("dd MMMM yyyy"),
                MemberQuoteSearchedRetirementAge = GetAge(calculation.EffectiveRetirementDate, member.PersonalDetails.DateOfBirth.Value),
                MemberQuotePensionOptionNumber = 0,
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
                RootQuoteName = selectedQuoteName,
                Quotev2 = quotesV2,
                SummaryBlocks = summaryBlocks,
                CmsTokens = cmsToken,
                ContentBlocks = contentBlockItems,
            });
    }

    public async Task<string> Render(string template, JsonElement optionsData, JsonElement options, CmsTokenInformationResponse cmsToken, Calculation calculation, IEnumerable<JsonElement> contentBlocks, (string AccessToken, string Env, string Bgroup) auth)
    {
        var optionList = new List<object>();
        foreach (var property in options.EnumerateObject())
        {
            var (quotesV2, summaryBlocks) = await _retirementCalculationQuotes.Create(calculation, property.Name, new JsonElement(), auth);
            var optionsBlock = _retirementCalculationQuotes.FilterOptionsByKey(optionsData, calculation, property.Name);
            if (optionsBlock.IsNone)
                continue;
            int? optionNumber = optionsBlock.Value().OrderNo;
            if (TryExtractOptionNumber(property.Value, out int? optionNumberElement))
                optionNumber = optionNumberElement;

            var summaryObject = new
            {
                Quotev2 = quotesV2,
                optionsBlock.Value().Header,
                optionsBlock.Value().Description,
                OptionNumber = optionNumber,
                optionsBlock.Value().SummaryItems,
            };
            optionList.Add(summaryObject);
        }

        var contentBlockItems = _retirementCalculationQuotes.GetContentBlocks(contentBlocks, cmsToken);

        return await Template.Parse(template).RenderAsync(new
        {
            OptionList = optionList.OrderBy(item => ((dynamic)item).OptionNumber).ToList(),
            MemberQuotePensionOptionNumber = 0,
            CmsTokens = cmsToken,
            ContentBlocks = contentBlockItems,
        });
    }

    private bool TryExtractOptionNumber(JsonElement element, out int? optionNumber)
    {
        optionNumber = null;
        if (element.TryGetProperty("attributes", out var attributesElement) &&
            attributesElement.TryGetProperty("optionNumber", out var optionNumberElement) &&
            optionNumberElement.ValueKind == JsonValueKind.Number)
        {
            optionNumber = optionNumberElement.GetInt32();
            return true;
        }
        return false;
    }

    private int GetAge(DateTimeOffset searchedRetirementDate, DateTimeOffset dateOfBirth)
    {
        var age = searchedRetirementDate.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > searchedRetirementDate.AddYears(-age))
            age--;

        return age;
    }
}