using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.JourneysCheckboxes;

public record SaveCheckboxRequest
{
    [Required]
    [MaxLength(100)]
    public string Key { get; init; }
    public bool AnswerValue { get; init; }
}