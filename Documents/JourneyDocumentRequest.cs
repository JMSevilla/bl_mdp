using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Documents;

public class JourneyDocumentRequest
{
    [Required]
    public string JourneyType { get; set; }

    [Required]
    public IFormFile File { get; set; }
    public List<string> Tags { get; set; } = new();
    public string DocumentType { get; set; }
}
