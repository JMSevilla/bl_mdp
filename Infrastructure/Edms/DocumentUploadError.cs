using System.Collections.Generic;
using System.Text.Json.Serialization;
using WTW.Web.Errors;

namespace WTW.MdpService.Infrastructure.Edms;

public record DocumentUploadError
{
    [JsonPropertyName("message")]
    public string Message { get; init; }
}
    
