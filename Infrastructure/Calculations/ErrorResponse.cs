using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Calculations;

public record ErrorsResponse
{
    public IList<string> Fatals { get; init; } = new List<string>();
    public IList<string> Warnings { get; init; } = new List<string>();
}
