using System;

namespace WTW.MdpService.Journeys.Submit.Services.Dto;

public class SubmissionDetailsDto
{
    public string CaseNumber { get; set; }
    public int? SummaryPdfEdmsImageId { get; set; }
    public DateTimeOffset? SubmissionDate { get; set; }
}
