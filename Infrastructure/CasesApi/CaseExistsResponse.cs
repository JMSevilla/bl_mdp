namespace WTW.MdpService.Infrastructure.CasesApi;

public record CaseExistsResponse
{
    public bool CaseExists { get; init; }
}