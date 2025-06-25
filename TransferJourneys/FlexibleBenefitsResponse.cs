using System;

namespace WTW.MdpService.TransferJourneys;

public record FlexibleBenefitsResponse
{
    public FlexibleBenefitsResponse(string nameOfPlan, string typeOfPayment, DateTime? dateOfPayment)
    {
        NameOfPlan = nameOfPlan;
        TypeOfPayment = typeOfPayment;
        DateOfPayment = dateOfPayment;
    }

    public string NameOfPlan { get; }
    public string TypeOfPayment { get; }
    public DateTime? DateOfPayment { get; }
}