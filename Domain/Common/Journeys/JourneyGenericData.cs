namespace WTW.MdpService.Domain.Common.Journeys;

public class JourneyGenericData
{
    protected JourneyGenericData() { }

    public JourneyGenericData(string genericDataJson, string formKey)
    {
        GenericDataJson = genericDataJson;
        FormKey = formKey;
    }

    public string GenericDataJson { get; }
    public string FormKey { get; }
    
    public JourneyGenericData Duplicate()
    {
        return new(GenericDataJson, FormKey);
    }
}