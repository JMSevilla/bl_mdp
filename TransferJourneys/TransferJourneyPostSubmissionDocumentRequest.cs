using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.TransferJourneys;

public class TransferJourneyPostSubmissionDocumentRequest
{
    public string JourneyType { get; set; }
}
