using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.RetryPolicy
{
    public class RetryPolicyOptions
    {
        [Required]
        public int GeneralRetryCount { get; set; }
        [Required]
        public int GeneralRetryDelay { get; set; }
        [Required]
        public int RetryCountFor425 { get; set; }
        [Required]
        public int RetryDelayFor425 { get; set; }
    }
}
