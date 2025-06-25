using System;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members;

public class NotificationSetting
{
    protected NotificationSetting() { }

    private NotificationSetting(long sequenceNumber, DateTimeOffset utcNow, string onlineCommunicationConsent, string communicationPreference)
    {
        SequenceNumber = sequenceNumber;
        StartDate = utcNow;
        OnlineCommunicationConsent = onlineCommunicationConsent;
        Settings = communicationPreference;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public long SequenceNumber { get; }
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset? EndDate { get; private set; }
    public string OnlineCommunicationConsent { get; }
    public string Settings { get; }
    public string Scheme { get; } = "M";

    public (bool Email, bool Sms, bool Post) NotificationSettings()
    {
        return (
            !string.IsNullOrWhiteSpace(Settings) && (Settings.Equals("E", StringComparison.OrdinalIgnoreCase) || Settings.Equals("B", StringComparison.OrdinalIgnoreCase)),
            !string.IsNullOrWhiteSpace(Settings) && (Settings.Equals("S", StringComparison.OrdinalIgnoreCase) || Settings.Equals("B", StringComparison.OrdinalIgnoreCase)),
            string.IsNullOrWhiteSpace(Settings)
            );
    }

    public static Either<Error, NotificationSetting> Create(bool email, bool sms, bool post, long sequenceNumber, string typeToUpdate, DateTimeOffset utcNow)
    {
        if (!email && !sms && !post)
            return Error.New("At least one of notification preference must be selected.");

        if (typeToUpdate == "POST")
            return new NotificationSetting(sequenceNumber, utcNow, "N", null);

        if ((typeToUpdate == "SMS" || typeToUpdate == "EMAIL") && !email)
            return new NotificationSetting(sequenceNumber, utcNow, "Y", "S");

        if ((typeToUpdate == "SMS" || typeToUpdate == "EMAIL") && !sms)
            return new NotificationSetting(sequenceNumber, utcNow, "Y", "E");

        return new NotificationSetting(sequenceNumber, utcNow, "Y", "B");
    }

    public void Close(DateTimeOffset utcNow)
    {
        EndDate = utcNow.AddDays(-1);
    }
}
