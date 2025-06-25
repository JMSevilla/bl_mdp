using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Journeys.JourneysGenericData;

public class JourneyGenericDataResponse
{
    public JourneyGenericDataResponse(JourneyGenericData data)
    {
        GenericDataJson = data.GenericDataJson;
        FormKey = data.FormKey;
    }

    public string FormKey { get; }
    public string GenericDataJson { get; }
}
