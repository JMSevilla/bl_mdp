using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record FlexibleBenefitsRequest
{
    [MaxLength(50)]
    public string NameOfPlan { get; init; }

    [MaxLength(50)]
    public string TypeOfPayment { get; init; }


    [DataType(DataType.Date)]
    public DateTime? DateOfPayment { get; init; }
}