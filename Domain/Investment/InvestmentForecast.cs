namespace WTW.MdpService.Domain.Investment;

public class InvestmentForecast
{
    public InvestmentForecast(
        int age,
        decimal assetValue,
        decimal taxFreeCash)
    {
        Age = age;
        AssetValue = assetValue;
        TaxFreeCash = taxFreeCash;
    }

    public int Age { get; }
    public decimal AssetValue { get; }
    public decimal TaxFreeCash { get; }
}