using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService;

public class JourneyPdfDownloadRequest
{
    [Required]
    public string TemplateName { get; set; }

    [Required]
    public string ContentAccessKey { get; set; }
}