#nullable enable
using System;

namespace WTW.MdpService.Infrastructure.BankService;

public class GetBankAccountClientResponse
{
    public string Bgroup { get; set; } = null!;
    public string Refno { get; set; } = null!;
    public decimal Seqno { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? BicCode { get; set; }
    public string? IbanNumber { get; set; }
    public string? LocalSortCode { get; set; }
    public string? OverseasSortCode { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BuildingSocietyRollNumber { get; set; }
    public string? AccountName { get; set; }
    public string? BankAccountCurrency { get; set; }
    public string? MemberAddressLine1 { get; set; }
    public string? MemberCity { get; set; }
    public string? MemberState { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? BankName { get; set; }
    public string? BankCity { get; set; }
    public string? BankState { get; set; }
    public string? BankCountryCode { get; set; }
    public string? IntermediaryBicCode { get; set; }
    public string? IntermediaryBankAccountNumber { get; set; }
    public string? PaymentType { get; set; }
    public string? SecurityChecksum { get; set; }
    public DateTime? DateEntered { get; set; }
    public string? TimeEntered { get; set; }
    public string? DataSource { get; set; }
    public string? Caseno { get; set; }
    public string? IntermediaryBankName { get; set; }
    public string? IntermediaryBankCountry { get; set; }
}
#nullable disable
