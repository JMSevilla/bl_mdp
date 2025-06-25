namespace WTW.MdpService.Domain.Members;

public enum RetirementApplicationStatus
{
    NotEligibleToStart = 1,
    EligibleToStart = 2,
    StartedRA = 3,
    ExpiredRA = 4,
    SubmittedRA = 5,
    RetirementCase = 6,
    TransferCase = 7,
    RetirementDateOutOfRange = 8,
    Undefined = 9,
    MinimumRetirementDateOutOfRange = 10
}