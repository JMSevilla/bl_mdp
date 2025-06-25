using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public record SchemeResponse
{
    public SchemeResponse(Member member)
    {
        Code = member.SchemeCode;
        Name = member.Scheme.Name;
        Type = member.Scheme.Type;
    }

    public string Code { get; }
    public string Type { get; }
    public string Name { get; }
}