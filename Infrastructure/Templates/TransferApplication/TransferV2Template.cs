using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Scriban;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.Web;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public class TransferV2Template : ITransferV2Template
{
    private readonly ICalculationsParser _calculationsParser;

    public TransferV2Template(ICalculationsParser calculationsParser)
    {
        _calculationsParser = calculationsParser;
    }
    public async Task<string> RenderHtml(
        string htmlTemplate,
        TransferJourney journey,
        Member member,
        TransferQuote transferQuote,
        TransferApplicationStatus transferApplicationStatus,
        DateTimeOffset now,
        IEnumerable<UploadedDocument> documents,
        Option<Calculation> retirementCalculation)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(new
        {
            StartDate = journey.StartDate,
            SystemDate = now,
            SubmissionDate = journey.SubmissionDate,
            PensionWiseDate = journey.PensionWiseDate,
            FinancialAdviseDate = journey.FinancialAdviseDate,
            TransferGuaranteeExpiryDate = transferQuote.GuaranteeDate,
            TotalGuaranteedTransferValue = transferQuote.TransferValues.TotalGuaranteedTransferValue,
            TotalNonGuaranteedTransferValue = transferQuote.TransferValues.TotalNonGuaranteedTransferValue,
            TotalTransferValue = transferQuote.TransferValues.TotalTransferValue,
            TransferApplicationStatus = transferApplicationStatus.ToString(),
            MemberEmail = member.Email().SingleOrDefault(),
            MemberTitle = member.PersonalDetails.Title,
            MemberForenames = member.PersonalDetails.Forenames,
            MemberSurname = member.PersonalDetails.Surname,
            MemberDateOfBirth = member.PersonalDetails.DateOfBirth,
            MemberPhone = member.FullMobilePhoneNumber().SingleOrDefault(),
            UploadedDocuments = documents.Select(d => d.FileName),
            IdentityUploadedDocuments = documents.Where(d => d.JourneyType == MdpConstants.IdentityDocumentType).Select(d => d.FileName),
            MemberQuoteHasAvcs = retirementCalculation.MatchUnsafe(r => !string.IsNullOrEmpty(r.RetirementJsonV2) ? _calculationsParser.GetRetirementV2(r.RetirementJsonV2).HasAdditionalContributions() : false, () => false),
            MemberAddress = member.Address().IsNone ? null :
                   new
                   {
                       Line1 = member.Address().Value().StreetAddress1,
                       Line2 = member.Address().Value().StreetAddress2,
                       Line3 = member.Address().Value().StreetAddress3,
                       Line4 = member.Address().Value().StreetAddress4,
                       Line5 = member.Address().Value().StreetAddress5,
                       Country = member.Address().Value().Country,
                       CountryCode = member.Address().Value().CountryCode,
                       PostCode = member.Address().Value().PostCode
                   },
            TransferJourneyContacts = journey.Contacts.Select(c => new
            {
                Type = c.Type,
                Name = c.Name,
                AdviserName = c.AdvisorName,
                SchemeName = c.SchemeName,
                CompanyName = c.CompanyName,
                Email = c.Email.Address,
                PhoneCode = c.Phone?.Code(),
                PhoneNumber = c.Phone?.Number(),
                Address = new
                {
                    Line1 = c.Address.StreetAddress1,
                    Line2 = c.Address.StreetAddress2,
                    Line3 = c.Address.StreetAddress3,
                    Line4 = c.Address.StreetAddress4,
                    Line5 = c.Address.StreetAddress5,
                    Country = c.Address.Country,
                    CountryCode = c.Address.CountryCode,
                    PostCode = c.Address.PostCode
                }
            }),
            QuestionForms = journey.QuestionForms(new string[0]).Select(q => new
            {
                QuestionKey = q.QuestionKey,
                AnswerKey = q.AnswerKey,
                AnswerValue = q.AnswerValue,
            }),
            FlexibleBenefits = new
            {
                NameOfPlan = journey.NameOfPlan,
                TypeOfPayment = journey.TypeOfPayment,
                DateOfPayment = journey.DateOfPayment,
            },
            CheckboxesLists = journey.CheckBoxesLists().Select(c => new
            {
                CheckboxesListKey = c.CheckboxesListKey,
                Checkboxes = c.Checkboxes.Select(x => new
                {
                    Key = x.Key,
                    AnswerValue = x.AnswerValue
                })
            }),
            GenericDataList = journey.GetJourneyStepsWithGenericData().Select(d => new
            {
                PageKey = d.CurrentPageKey,
                Forms = d.JourneyGenericDataList.Select(g => new
                {
                    FormKey = g.FormKey,
                    GenericData = g.GenericDataJson
                })
            }),
        });
    }
}