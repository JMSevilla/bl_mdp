using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.QuoteSelectionJourneys;

public record SubmitQuoteSelectionStepRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(1000)]
    public string SelectedQuoteName { get; init; }
}