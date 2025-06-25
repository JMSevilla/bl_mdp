using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using System.Linq;
using static WTW.MdpService.BereavementJourneys.BereavementJourneySubmitRequest;

namespace WTW.MdpService.Infrastructure.Templates.Bereavement
{
    public class BereavementTemplate : IBereavementTemplate
    {
        public async Task<(string,string)> RenderHtml(
            string htmlTemplate,
            IEnumerable<QuestionForm> questionForms,
            BereavementJourneyDeceasedPerson deceasedPerson,
            BereavementJourneyPerson reporter,
            BereavementJourneyPerson nextOfKin,
            BereavementJourneyPerson executor,
            BereavementJourneyPerson contactPerson,
            DateTimeOffset now,
            IEnumerable<UploadedDocument> documents)
        {
            var renderedTemplate = await Template.Parse(htmlTemplate).RenderAsync(
                new
                {
                    DateNow = now,
                    Deceased = deceasedPerson,
                    Reporter = reporter,
                    NextOfKin = nextOfKin,
                    Executor = executor,
                    Contact = contactPerson,
                    JourneyQuestions = questionForms,
                    UploadedDocuments = documents.Select(d => d.FileName),
                });

            var (fileName, template) = GetFileNameFromTemplate(renderedTemplate);
            return (fileName, template);
        }

        private static (string, string) GetFileNameFromTemplate(string template)
        {
            var templateParts = template.Split(new string[] { "[[filename:<", ">]]" }, StringSplitOptions.RemoveEmptyEntries);
            return (templateParts[0], templateParts[1]);
            
        }
    }
}
