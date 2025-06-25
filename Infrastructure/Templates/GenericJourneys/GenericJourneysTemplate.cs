using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using System.Linq;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Templates.GenericJourneys;

public class GenericJourneysTemplate : IGenericJourneysTemplate
{
    public async Task<string> RenderHtml(
        string htmlTemplate,
        GenericJourney journey,
        Member member,
        DateTimeOffset now,
        string caseNumber,
        IEnumerable<SummaryBlock> summaryBlocks,
        IEnumerable<ContentBlockItem> contentBlocks)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(new
        {
            SystemDate = now,
            MemberData = new
            {
                Title = member.PersonalDetails.Title,
                Forenames = member.PersonalDetails.Forenames,
                Surname = member.PersonalDetails.Surname,
                DateOfBirth = member.PersonalDetails.DateOfBirth,
                Email = member.Email().SingleOrDefault(),
                Phone = member.FullMobilePhoneNumber().SingleOrDefault(),
                Address = member.Address().IsNone ? null :
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
            },
            JourneyData = new
            {
                StartDate = journey.StartDate,
                JourneyType = journey.Type,
                JourneyStatus = journey.Status,
                SubmissionDate = journey.SubmissionDate,
                ExpirationDate = journey.ExpirationDate,
                CaseNumber = caseNumber,
                QuestionForms = journey.QuestionForms(new string[0]).Select(q => new
                {
                    QuestionKey = q.QuestionKey,
                    AnswerKey = q.AnswerKey,
                    AnswerValue = q.AnswerValue,
                }),
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
            },
            SummaryBlocks = summaryBlocks,
            ContentBlocks = contentBlocks
        });
    }

    public async Task<string> RenderHtml(string htmlTemplate, object templateData)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(templateData);
    }
}