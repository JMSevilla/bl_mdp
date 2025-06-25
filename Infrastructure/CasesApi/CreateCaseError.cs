using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public record CreateCaseError
{
    public CreateCaseInnerError Errors { get; init; }
    public string Message { get; init; }

    public record CreateCaseInnerError
    {
        [JsonPropertyName("")]
        public string Message { get; init; }
    }
}