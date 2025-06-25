namespace WTW.MdpService.ContactsConfirmation;

public record ContactsConfirmationConfiguration(
    int EmailTokenExpiresInMin,
    int MobilePhoneTokenExpiresInMin,
    int MaxMobileConfirmationAttemptCount,
    int MaxEmailConfirmationAttemptCount);