using System;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Mdp;

public class ContactConfirmation
{
    private const string TokenExpiredErrorCode = "CONFIRMATION_TOKEN_EXPIRED_ERROR";
    private const string TokenInvalidErrorCode = "CONFIRMATION_TOKEN_INVALID_ERROR";
    protected ContactConfirmation() { }

    private ContactConfirmation(string businessGroup,
        string referenceNumber,
        string token,
        string contact,
        ContactType contactType,
        DateTimeOffset expiresAt,
        DateTimeOffset utcNow,
        int maximumConfirmationAttemptCount)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        Token = token;
        Contact = contact;
        ContactType = contactType;
        CreatedAt = utcNow;
        ExpiresAt = expiresAt;
        FailedConfirmationAttemptCount = 0;
        MaximumConfirmationAttemptCount = maximumConfirmationAttemptCount;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string Token { get; }
    public ContactType ContactType { get; }
    public string Contact { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? ValidatedAt { get; private set; }
    public int FailedConfirmationAttemptCount { get; private set; }
    public int? MaximumConfirmationAttemptCount { get; }

    public static ContactConfirmation CreateForEmail(string businessGroup,
        string referenceNumber,
        string token,
        Email email,
        DateTimeOffset expiresAt,
        DateTimeOffset utcNow,
        int maximumEmailConfirmationCount)
    {
        if (expiresAt <= utcNow)
            throw new InvalidOperationException();

        return new ContactConfirmation(businessGroup, referenceNumber, token, email, ContactType.EmailAddress,
            expiresAt, utcNow, maximumEmailConfirmationCount);
    }

    public Error? MarkValidated(string token, DateTimeOffset utcNow, bool IsOtpDisabled)
    {
        if (TokenExceededMaxFailureCount)
            return Error.New(TokenExpiredErrorCode);

        if ((Token != token || ValidatedAt != null || ExpiresAt < utcNow) && !IsOtpDisabled)
        {
            FailedConfirmationAttemptCount++;
            return Error.New(TokenExceededMaxFailureCount ? TokenExpiredErrorCode : TokenInvalidErrorCode);
        }

        ValidatedAt = utcNow;
        return null;
    }

    public static ContactConfirmation CreateForMobile(string businessGroup,
        string referenceNumber,
        string token,
        Phone phone,
        DateTimeOffset expiresAt,
        DateTimeOffset utcNow,
        int maximumMobileConfirmationCount)
    {
        if (expiresAt <= utcNow)
            throw new InvalidOperationException();

        return new ContactConfirmation(businessGroup, referenceNumber, token, phone, ContactType.MobilePhoneNumber,
            expiresAt, utcNow, maximumMobileConfirmationCount);
    }
    
    private bool TokenExceededMaxFailureCount => FailedConfirmationAttemptCount == MaximumConfirmationAttemptCount;
}

public enum ContactType
{
    EmailAddress,
    MobilePhoneNumber
}