using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public record StatutoryFactors
{
    [JsonPropertyName("normalMinimumPensionAge")]
    public string NormalMinimumPensionAge { get; init; }

    [JsonPropertyName("standardLifetimeAllowance")]
    public decimal StandardLifetimeAllowance { get; init; }
}