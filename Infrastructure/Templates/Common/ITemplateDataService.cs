using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Infrastructure.Templates.GenericJourneys;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.Common;

public interface ITemplateDataService
{
    CmsTokenInformationResponse GetCmsTokensResponseData(Member member, Option<Calculation> calculationOption);
    Task<IEnumerable<ContentBlockItem>> GetGenericContentBlockItems(TemplateResponse template, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth);
    Task<IEnumerable<SummaryBlock>> GetGenericDataSummaryBlocks(string dataSummaryBlockKey, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth);
    Task<IEnumerable<DataSummaryItem>> GetGenericDataSummaryBlocks(IEnumerable<string> dataSummaryBlockKeys, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth);
    Task<IOrderedEnumerable<OptionListItem>> GetOptionListItems(Calculation calculation, string accessKey);
    Task<IEnumerable<SummaryBlock>> GetOptionSummaryDataSummaryBlocks(Calculation calculation, string contentAccessKey);
}