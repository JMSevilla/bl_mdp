using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

namespace WTW.MdpService.Infrastructure.Templates.GenericJourneys;

public interface IGenericJourneysTemplate
{
    Task<string> RenderHtml(string htmlTemplate, object templateData);
    Task<string> RenderHtml(string htmlTemplate, GenericJourney journey, Member member, DateTimeOffset now, string caseNumber, IEnumerable<SummaryBlock> summaryBlocks, IEnumerable<ContentBlockItem> contentBlocks);
}