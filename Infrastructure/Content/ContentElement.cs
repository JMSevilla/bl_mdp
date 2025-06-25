using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Content;

public class ContentElement
{
    public ContentBusinessGroup BusinessGroup { get; set; }

    public ContentWebRuleWordingFlag WebRuleWordingFlag { get; set; }

    public CmsConfiguredWordingFlags CmsConfiguredWordingFlags { get; set; }

    [JsonPropertyName("blockSingleAuthWelcomeEmail")]
    public ContentFlag BlockSingleAuthWelcomeEmail { get; set; }


    [JsonPropertyName("blockSaRelatedMemberDataRegistration")]
    public ContentFlag BlockSaRelatedMemberDataRegistration { get; set; }
}
