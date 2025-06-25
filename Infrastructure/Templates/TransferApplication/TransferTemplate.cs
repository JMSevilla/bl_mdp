using System;
using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public static class TransferTemplate
{
    public static async Task<string> RenderHtml(string htmlTemplate, PartialTransferResponse.MdpResponse partialTransferMdp, string referenceNumber, DateTimeOffset now)
    {
        var details = Details(partialTransferMdp, referenceNumber, now);

        return await Template.Parse(htmlTemplate).RenderAsync(
            new
            {
                PtMemberRefno = referenceNumber,
                PtGeneratedDate = $"{details.PtGeneratedDate:dd MMMM yyyy}",

                FpPre88Gmp = details.FpPre88Gmp,
                FpPost88Gmp = details.FpPost88Gmp,
                FpPre97Excess = details.FpPre97Excess,
                FpPost97 = details.FpPost97,
                FpTotalPension = details.FpTotalPension,

                FtTransferGmp = details.FtTransferGmp,
                FtPre97Excess = details.FtPre97Excess,
                FtPost97 = details.FtPost97,
                FtTotalTransfer = details.FtTotalTransfer,

                PtTransferGmp = details.PtTransferGmp,
                PtPre97Excess = details.PtPre97Excess,
                PtPost97 = details.PtPost97,
                PtTotalTransfer = details.PtTotalTransfer,

                RpDeferredPension = details.RpDeferredPension,
                RpPre97Excess = details.RpPre97Excess,
                RpPost97 = details.RpPost97,
                RpTotalPension = details.RpTotalPension
            });
    }

    private static TransferApplicationTemplateDetails Details(PartialTransferResponse.MdpResponse partialTransferMdp, string referenceNumber, DateTimeOffset now)
    {
        return new()
        {
            MemberQuoteReferenceNumber = referenceNumber,
            PtGeneratedDate = now,

            FpPre88Gmp = partialTransferMdp?.PensionTranchesFull.Pre88Gmp,
            FpPost88Gmp = partialTransferMdp?.PensionTranchesFull.Post88Gmp,
            FpPre97Excess = partialTransferMdp?.PensionTranchesFull.Pre97Excess,
            FpPost97 = partialTransferMdp?.PensionTranchesFull.Post97,
            FpTotalPension = partialTransferMdp?.PensionTranchesFull.Total,

            FtTransferGmp = partialTransferMdp?.TransferValuesFull.Gmp,
            FtPre97Excess = partialTransferMdp?.TransferValuesFull.Pre97Excess,
            FtPost97 = partialTransferMdp?.TransferValuesFull.Post97,
            FtTotalTransfer = partialTransferMdp?.TransferValuesFull.Total,

            PtTransferGmp = partialTransferMdp?.TransferValuesPartial.Gmp,
            PtPre97Excess = partialTransferMdp?.TransferValuesPartial.Pre97Excess,
            PtPost97 = partialTransferMdp?.TransferValuesPartial.Post97,
            PtTotalTransfer = partialTransferMdp?.TransferValuesPartial.Total,

            RpDeferredPension = partialTransferMdp?.PensionTranchesResidual.Gmp,
            RpPre97Excess = partialTransferMdp?.PensionTranchesResidual.Pre97Excess,
            RpPost97 = partialTransferMdp?.PensionTranchesResidual.Post97,
            RpTotalPension = partialTransferMdp?.PensionTranchesResidual.Total
        };
    }
}