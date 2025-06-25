using System;

namespace WTW.MdpService.Journeys.Submit.Services.Dto;

public record DcSubmissionRetirementDateDto
{
    public DateTimeOffset RetirementDate { get; init; }
}