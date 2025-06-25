using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class QuoteV2
{
    public QuoteV2(string name,
        IEnumerable<QuoteAttributesV2> attributes,
        IEnumerable<PensionTranche> pensionTranches)
    {
        Name = name;
        Attributes = attributes;
        PensionTranches = pensionTranches;
    }

    public string Name { get; }
    public IEnumerable<QuoteAttributesV2> Attributes { get; }
    public IEnumerable<PensionTranche> PensionTranches { get; }
}

public class QuoteAttributesV2
{
    public QuoteAttributesV2(string name, decimal? value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public decimal? Value { get; }
}