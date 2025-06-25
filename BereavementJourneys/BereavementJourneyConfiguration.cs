namespace WTW.MdpService.BereavementJourneys;

public record BereavementJourneyConfiguration(
    int ValidityPeriodInMin,
    int ExpiredJourneysRemovalPeriodInMin,
    int EmailTokenExpiresInMin,
    int MaxEmailConfirmationAttemptCount,
    int EmailLockPeriodInMin,
    int FailedJourneyValidityPeriodInMin);