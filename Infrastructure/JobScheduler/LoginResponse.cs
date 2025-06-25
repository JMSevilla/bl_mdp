namespace WTW.MdpService.Infrastructure.JobScheduler;

public record LoginResponse
{
    public string AccessToken { get; init; }
}