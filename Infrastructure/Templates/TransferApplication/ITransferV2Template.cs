using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public interface ITransferV2Template
{
    Task<string> RenderHtml(
        string htmlTemplate,
        TransferJourney journey,
        Member member, 
        TransferQuote transferQuote,
        TransferApplicationStatus transferApplicationStatus,
        DateTimeOffset now,
        IEnumerable<UploadedDocument> documents,
        Option<Calculation> retirementCalculation);
}