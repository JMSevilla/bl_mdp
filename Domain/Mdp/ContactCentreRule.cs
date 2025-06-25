using System;
using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp
{
    public partial class ContactCentreRule
    {
        public string BusinessGroup { get; set; }
        public string Scheme { get; set; }
        public string MemberStatus { get; set; }
        public string WebchatFlag { get; set; }
        public string UserIdList { get; set; }
        public string WebChatUrl { get; set; }
        public string RedirectPage { get; set; }
        public string RequestTranscript { get; set; }
        public string RequestSurvey { get; set; }
        public string Ddi { get; set; }
        public string EmergencyRedirectPage { get; set; }
        public string HolidayRedirectPage { get; set; }
    }
}
