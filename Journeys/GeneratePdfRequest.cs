using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys
{
    public class GeneratePdfRequest
    {
        public string CaseType { get; set; }
        public string JourneyType { get; set; }
        [Required]
        public string CaseNumber { get; set; }

        [Required]
        public string ContentAccessKey { get; set; }
    }
}
