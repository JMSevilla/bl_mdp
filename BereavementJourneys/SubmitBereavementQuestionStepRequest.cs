using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BereavementJourneys;

public record SubmitBereavementQuestionStepRequest
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
    [MaxLength(25)]
    public string QuestionKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string AnswerKey { get; init; }

    [Required]
    public string AnswerValue { get; init; }

    public bool AvoidBranching { get; init; }
}