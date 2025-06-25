namespace WTW.MdpService.Domain.Members;
public class Scheme
{
    protected Scheme() { }

    public Scheme(string baseCurrency, string name, string type)
    {
        BaseCurrency = baseCurrency;
        Name = name;
        Type = type;
    }

    public string Type { get; }
    public string Name { get; }
    public string BaseCurrency { get; }
}