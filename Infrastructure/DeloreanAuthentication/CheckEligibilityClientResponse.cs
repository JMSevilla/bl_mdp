namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class CheckEligibilityClientResponse
{
    public bool Eligible { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
}
