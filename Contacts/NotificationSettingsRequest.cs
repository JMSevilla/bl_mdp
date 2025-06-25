namespace WTW.MdpService.Contacts;

public record NotificationSettingsRequest(NotificationType TypeToUpdate, bool Email, bool Sms, bool Post);

public enum NotificationType
{
    EMAIL,
    SMS,
    POST
}