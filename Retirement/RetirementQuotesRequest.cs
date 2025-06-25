using System;

namespace WTW.MdpService.Retirement;

public class RetirementQuotesRequest
{
    public DateTime? SelectedRetirementDate { get; set; }
    public bool BypassCache { get; set; } = false;
}
