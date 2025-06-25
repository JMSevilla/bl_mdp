using System.Collections.Generic;

namespace WTW.MdpService.Domain.Common.Journeys;

public class GenericJourneyStagePage
{
    public IEnumerable<string> StageStartSteps { get; set; }
    public IEnumerable<string> StageEndSteps { get; set; }
}
