using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys.Submit;

public record SubmitQuoteRequestCaseRequest
{
    [Required]
    public string CaseType { get; init; }

    [Required]
    public string AccessKey { get; init; }
}