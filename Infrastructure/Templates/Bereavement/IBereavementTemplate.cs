using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.BereavementJourneys;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.Templates.Bereavement;

public interface IBereavementTemplate
{
    Task<(string,string)> RenderHtml(
        string htmlTemplate,
        IEnumerable<QuestionForm> questionForms,
        BereavementJourneySubmitRequest.BereavementJourneyDeceasedPerson deceasedPerson,
        BereavementJourneySubmitRequest.BereavementJourneyPerson reporter,
        BereavementJourneySubmitRequest.BereavementJourneyPerson nextOfKin,
        BereavementJourneySubmitRequest.BereavementJourneyPerson executor,
        BereavementJourneySubmitRequest.BereavementJourneyPerson contactPerson,
        DateTimeOffset now,
        IEnumerable<UploadedDocument> documents);
}