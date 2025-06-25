using System.Collections.Generic;

namespace WTW.MdpService.Members;

public record AgeLinesResponse
{
    public IEnumerable<int> AgeLines { get; init; }

    public static AgeLinesResponse From(IEnumerable<int> ageLines)
    {
        return new()
        {
            AgeLines = ageLines
        };
    }
}