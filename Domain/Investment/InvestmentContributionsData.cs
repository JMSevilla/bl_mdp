namespace WTW.MdpService.Domain.Investment;

public class InvestmentContributionsData
{
    public InvestmentContributionsData(
        string code,
        string name,
        decimal paidIn,
        decimal value)
    {
        Code = code;
        Name = name;
        PaidIn = paidIn;
        Value = value;
    }

    public string Code { get; set; }
    public string Name { get; set; }
    public decimal PaidIn { get; set; }
    public decimal Value { get; set; }
}