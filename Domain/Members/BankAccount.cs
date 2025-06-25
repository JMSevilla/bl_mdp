using System;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members;

public class BankAccount
{
    protected BankAccount() { }

    private BankAccount(
        int sequenceNumber,
        string accountName,
        string accountNumber,
        string iban,
        DateTimeOffset utcNow,
        Bank bank)
    {
        SequenceNumber = sequenceNumber;
        AccountName = accountName;
        AccountNumber = accountNumber;
        Iban = iban;
        Bank = bank;
        EffectiveDate = utcNow;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string AccountName { get; }
    public string AccountNumber { get; }
    public string Iban { get; }
    public DateTimeOffset? EffectiveDate { get; }
    public virtual Bank Bank { get; }
    public int SequenceNumber { get; }

    public static Either<Error, BankAccount> CreateUkAccount(int sequenceNumber, string accountName, string accountNumber, DateTimeOffset utcNow, Bank bank)
    {
        if (string.IsNullOrWhiteSpace(accountName) || accountName.Length > 40)
            return Error.New("Invalid account name: Must be between 1 and 40 digits length.");

        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length != 8)
            return Error.New("Invalid account number: Must be 8 digits length.");

        if (string.IsNullOrWhiteSpace(bank.SortCode))
            throw new ArgumentException("Bank with valid sort code must be provided to create uk bank account.");

        if (sequenceNumber < 1)
            throw new ArgumentException("Invalid sequence number.");

        return new BankAccount(sequenceNumber, accountName, accountNumber, default, utcNow, bank);
    }

    public static Either<Error, BankAccount> CreateIbanAccount(int sequenceNumber, string accountName, string iban, DateTimeOffset utcNow, Bank bank)
    {
        if (string.IsNullOrWhiteSpace(accountName) || accountName.Length > 40)
            return Error.New("Invalid account name: Must be between 1 and 40 digits length.");

        if (string.IsNullOrWhiteSpace(iban) || iban.Length > 34)
            return Error.New("Invalid IBAN: Must be between 1 and 34 digits length.");

        if (string.IsNullOrWhiteSpace(bank.Bic))
            throw new ArgumentException("Bank with valid BIC must be provided to create international bank account.");

        if (sequenceNumber < 1)
            throw new ArgumentException("Invalid sequence number.");

        return new BankAccount(sequenceNumber, accountName, default, iban, utcNow, bank);
    }
}