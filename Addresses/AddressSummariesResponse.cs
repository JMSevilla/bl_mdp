using WTW.MdpService.Infrastructure.Geolocation;

namespace WTW.MdpService.Addresses;

public record AddressSummariesResponse
{
    public string Highlight { get; init; }
    public string AddressId { get; init; }
    public string Text { get; init; }
    public string Type { get; init; }
    public string Description { get; init; }

    public static AddressSummariesResponse From(LocationApiAddressSummary addressSummary)
    {
        return new()
        {
            AddressId = addressSummary.Id,
            Highlight = addressSummary.Highlight,
            Text = addressSummary.Text,
            Type = addressSummary.Type,
            Description = addressSummary.Description
        };
    }
}