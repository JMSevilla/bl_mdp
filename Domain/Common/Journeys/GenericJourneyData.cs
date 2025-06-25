using System;
using System.Collections.Generic;
using System.Text.Json;

namespace WTW.MdpService.Domain.Common.Journeys;

public class GenericJourneyData
{
    public string Type { get; set; }
    public string Status { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public IDictionary<string, object> PreJourneyData { get; set; }
    public IDictionary<string, object> StepsWithData { get; set; }
    public IDictionary<string, object> StepsWithQuestion { get; set; }
    public IDictionary<string, object> StepsWithCheckboxes { get; set; }
}