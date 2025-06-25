using System;
using LanguageExt.Common;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class TransferCalculation
{
    protected TransferCalculation() { }

    public TransferCalculation(string businessGroup, string referenceNumber, string transferQuoteJson, DateTimeOffset utcNow)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        TransferQuoteJson = transferQuoteJson;
        CreatedAt = utcNow;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string TransferQuoteJson { get; }
    public DateTimeOffset CreatedAt { get; }
    public bool HasLockedInTransferQuote { get; private set; }
    public TransferApplicationStatus? Status { get; private set; }

    public void LockTransferQoute()
    {
        HasLockedInTransferQuote = true;
    }

    public void UnlockTransferQoute()
    {
        HasLockedInTransferQuote = false;
    }

    public Error? SetStatus(TransferApplicationStatus status)
    {
        if (!HasLockedInTransferQuote)
            return Error.New("Transfer journey must be started");

        Status = status;

        return null;
    }

    public TransferApplicationStatus TransferStatus()
    {
        if (HasLockedInTransferQuote && Status != null && Status.Value.Equals(TransferApplicationStatus.SubmitStarted))
            return TransferApplicationStatus.SubmitStarted;

        if (HasLockedInTransferQuote && Status != null && Status.Value.Equals(TransferApplicationStatus.SubmittedTA))
            return TransferApplicationStatus.SubmittedTA;

        if (HasLockedInTransferQuote && Status != null && Status.Value.Equals(TransferApplicationStatus.OutsideTA))
            return TransferApplicationStatus.OutsideTA;

        if (HasLockedInTransferQuote)
            return TransferApplicationStatus.StartedTA;

        return string.IsNullOrWhiteSpace(TransferQuoteJson) ? TransferApplicationStatus.UnavailableTA : TransferApplicationStatus.NotStartedTA;
    }
}