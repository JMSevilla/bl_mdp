using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using MessageBird;
using WTW.MdpService.ContactsConfirmation;

namespace WTW.MdpService.Infrastructure.SmsConfirmation;

public class MessageBirdClient : IMessageClient
{
    private const string DefaultOrginator = "MDP";
    private readonly Client _client;

    public MessageBirdClient(string accessKey)
    {
        _client = Client.CreateDefault(accessKey);
    }

    /// <summary> Allows to send SMS message. 
    /// <param name="originator">The sender of the message. This can be a telephone number (including country code) or an alphanumeric string. In case of an alphanumeric string, the maximum length is 11 characters.</param>
    /// <param name="body">	The body of the SMS message.</param>
    /// <param name="recipients">An array of recipients msisdns.</param>
    /// </summary> 
    public Either<Error, string> SendMessage(string originator, string body, string[] recipients)
    {
        var response = _client.SendMessage(ValidateOrginator(originator), body, recipients.Select(x => Convert.ToInt64(x.Replace(" ", string.Empty))).ToArray());

        // DO NOT CONVERT TO TERNARY - it will cause a bug
        if (response.Recipients.TotalDeliveryFailedCount > 0)
            return Error.New($"Some message delivery failed. Count: {response.Recipients.TotalDeliveryFailedCount}");

        return response.Id;
    }

    private string ValidateOrginator(string orginator)
    {
        if (orginator == null || orginator.Length > 11)
            return DefaultOrginator;

        return orginator;
    }
}