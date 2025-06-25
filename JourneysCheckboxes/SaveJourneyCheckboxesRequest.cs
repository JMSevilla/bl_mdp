using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.JourneysCheckboxes;

public record SaveJourneyCheckboxesRequest
{
    [Required]
    [MaxLength(100)]
    public string CheckboxesListKey { get; init; }

    [Required]
    [MinLength(1)]
    public IEnumerable<SaveCheckboxRequest> Checkboxes { get; init; }
}