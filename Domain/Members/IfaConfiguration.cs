namespace WTW.MdpService.Domain.Members;

public class IfaConfiguration
{
    protected IfaConfiguration() { }

    public IfaConfiguration(string businessGroup, string ifaName, string calculationType, string ifaEmail)
    {
        BusinessGroup = businessGroup;
        IfaName = ifaName;
        CalculationType = calculationType;
        IfaEmail = ifaEmail;
    }

    public string BusinessGroup { get; }
    public string IfaName { get; }
    public string CalculationType { get; }
    public string IfaEmail { get; }
}