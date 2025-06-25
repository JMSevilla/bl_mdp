using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTW.MdpService.TransferJourneys;

public record TransferJourneyContactAddressRequest
{
    [MaxLength(50)]
    public string Line1 { get; init; }

    [MaxLength(50)]
    public string Line2 { get; init; }

    [MaxLength(50)]
    public string Line3 { get; init; }

    [MaxLength(50)]
    public string Line4 { get; init; }

    [MaxLength(50)]
    public string Line5 { get; init; }

    [MaxLength(30)]
    public string Country { get; init; }

    [MaxLength(3)]
    public string CountryCode { get; init; }

    [MaxLength(8)]
    public string PostCode { get; init; }

    [Required]
    [MaxLength(50)]
    public string Type { get; init; }
}
