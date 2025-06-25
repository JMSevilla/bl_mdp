using System.Collections.Generic;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public record LinkedMemberResponse
{
    public IReadOnlyList<LinkedMember> LinkedMembers { get; init; }

    public static LinkedMemberResponse From(IReadOnlyList<LinkedMember> linkedMembers)
    {
        return new()
        {
            LinkedMembers = linkedMembers
        };
    }
}