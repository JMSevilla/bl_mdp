namespace WTW.MdpService.Domain.Investment;

public class InvestmentFund
{
    public InvestmentFund(
        string code,
        string name,
        decimal value)
    {
        Code = code;
        Name = name;
        Value = value;
    }

    public string Code { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
}