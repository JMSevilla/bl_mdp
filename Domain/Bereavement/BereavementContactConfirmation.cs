using System;
using LanguageExt.Common;
using LanguageExt.Pretty;
using WTW.MdpService.BereavementContactsConfirmation;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Domain.Bereavement;

public class BereavementContactConfirmation
{
    private const string TokenExpiredErrorCode = "CONFIRMATION_TOKEN_EXPIRED_ERROR";
    private const string TokenInvalidErrorCode = "CONFIRMATION_TOKEN_INVALID_ERROR";

    protected BereavementContactConfirmation() { }

    private BereavementContactConfirmation(string businessGroup,
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
        CreatedAt = utcNow;
        ExpiresAt = expiresAt;
        FailedConfirmationAttemptCount = 0;
        MaximumConfirmationAttemptCount = maximumConfirmationAttemptCount;
    }


    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string Token { get; private set; }
    public string Contact { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ExpiresAt { get; }
    public DateTimeOffset? ValidatedAt { get; private set; }
    public int FailedConfirmationAttemptCount { get; private set; }
    public int MaximumConfirmationAttemptCount { get; }

    public static BereavementContactConfirmation CreateForEmail(string businessGroup,
        Guid referenceNumber,
        string token,
        Email email,
        DateTimeOffset expiresAt,
        DateTimeOffset utcNow,
        int maximumEmailConfirmationCount)
    {
        if (expiresAt <= utcNow)
            throw new InvalidOperationException();

        return new BereavementContactConfirmation(businessGroup, referenceNumber.ToString(), token, EmailSecurity.Hash(email), ContactType.EmailAddress,
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

    public bool IsLockExpired(DateTimeOffset utcNow, int emailLockPeriodInMin)
    {
        return CreatedAt.AddMinutes(emailLockPeriodInMin) <= utcNow;
    }

    public DateTimeOffset LockReleaseDate(int emailLockPeriodInMin)
    {
        return CreatedAt.AddMinutes(emailLockPeriodInMin);
    }

    private bool TokenExceededMaxFailureCount => FailedConfirmationAttemptCount == MaximumConfirmationAttemptCount;
}

