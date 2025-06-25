namespace WTW.MdpService.Infrastructure.Geolocation;

public class LocationApiBaseResponse
{
    public string Error { get; set; }
    public string Cause { get; set; }
    public string Description { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);
}