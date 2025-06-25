using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Common;
using WTW.Web.Validation;

namespace WTW.MdpService.Documents;

public class JourneyDocumentTagUpdateRequest
{
    [Required]
    public string FileUuid { get; set; }

    [RequiredList]
    public List<string> Tags { get; set; } = new();
}
