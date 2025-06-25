namespace WTW.MdpService.Domain.Mdp.Calculations;

public class PensionTranche
{
    public PensionTranche(string trancheTypeCode, decimal value, string increaseTypeCode)
    {
        TrancheTypeCode = trancheTypeCode;
        Value = value;
        IncreaseTypeCode = increaseTypeCode;
    }

    public string TrancheTypeCode { get; }
    public string IncreaseTypeCode { get; }
    public decimal Value { get; }
}