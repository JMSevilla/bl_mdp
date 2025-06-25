using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys;

public record SubmitJourneyQuestionStepRequest
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
    [MaxLength(1000)]
    public string AnswerKey { get; init; }

    [MaxLength(1000)]
    public string AnswerValue { get; init; }
}
