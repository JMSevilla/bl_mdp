using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Documents;

public class CaseDocumentsRequest
{
    [Required]
    public string CaseCode { get; set; }
}
