using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Members.Beneficiaries;

public class BeneficiaryDetails : ValueObject
{
    public const string CharityStatus = "Charity";

    protected BeneficiaryDetails() { }

    private BeneficiaryDetails(
        string relationship,
        string forenames,
        string surName,
        DateTime? dateOfBirth,
        decimal lumpSumPercentage,
        decimal pensionPercentage,
        string notes)
    {
        Relationship = relationship;
        Forenames = forenames;
        MixedCaseSurname = surName;
        Surname = surName.ToUpper();
        DateOfBirth = dateOfBirth;
        LumpSumPercentage = lumpSumPercentage;
        PensionPercentage = pensionPercentage;
        Notes = notes;
    }

    private BeneficiaryDetails(
        string charityName,
        long charityNumber,
        decimal lumpSumPercentage,
        string notes)
    {
        Relationship = CharityStatus;
        CharityName = charityName;
        Surname = charityName.Length > 20 ? charityName.Substring(0, 20).ToUpper() : charityName.ToUpper();
        CharityNumber = charityNumber;
        LumpSumPercentage = lumpSumPercentage;
        Notes = notes;
    }

    public string Relationship { get; }
    public string Forenames { get; }
    public string Surname { get; }
    public string MixedCaseSurname { get; }
    public DateTimeOffset? DateOfBirth { get; }
    public string CharityName { get; }
    public long? CharityNumber { get; }
    public decimal? LumpSumPercentage { get; }
    public decimal? PensionPercentage { get; }
    public string Notes { get; }

    public static Either<Error, BeneficiaryDetails> CreateNonCharity(
        string relationship,
        string forenames,
        string surname,
        DateTime? dateOfBirth,
        decimal lumpSumPercentage,
        bool isPensionBeneficiary,
        string notes)
    {
        if (relationship is null)
            return Error.New($"{nameof(relationship)} is required.");

        if (relationship.Length > 20)
            return Error.New($"Relationship status max length 20 characters.");

        if (relationship.Equals(CharityStatus, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException();

        if (lumpSumPercentage < 0 || lumpSumPercentage > 100)
            return Error.New($"\"{nameof(lumpSumPercentage)}\" field must be between 0 and 100.");

        if (forenames is null || surname is null)
            return Error.New($"\"{nameof(forenames)}\" and \"{nameof(surname)}\" fields are required for non charity beneficiaries.");

        if (forenames.Length > 32)
            return Error.New($"Forenames max length 32 characters.");

        if (surname.Length > 20)
            return Error.New($"Surname max length 20 characters.");

        if (notes?.Length > 180)
            return Error.New($"Notes max length 180 characters.");

        if (relationship.HasHtmlTags() || forenames.HasHtmlTags() ||
            surname.HasHtmlTags() || notes.HasHtmlTags())
        {
            return Error.New(MdpConstants.InputContainingHTMLTagError);
        }

        return new BeneficiaryDetails(relationship, forenames, surname, dateOfBirth, lumpSumPercentage, isPensionBeneficiary ? 100 : 0, notes);
    }

    public static Either<Error, BeneficiaryDetails> CreateCharity(string charityName, long? charityNumber, decimal lumpSumPercentage, string notes)
    {
        if (lumpSumPercentage < 0 || lumpSumPercentage > 100)
            return Error.New($"\"{nameof(lumpSumPercentage)}\" field must be between 0 and 100.");

        if (charityName is null || charityNumber is null)
            return Error.New($"\"{nameof(charityName)}\" and \"{nameof(charityNumber)}\" fields are required for charity beneficiaries.");

        if (charityNumber < 1 || charityNumber > 9999999999)
            return Error.New($"\"{nameof(charityNumber)}\" value must be from 1 to 9999999999.");

        if (charityName.Length > 120)
            return Error.New($"Charity name max length 120 characters.");

        if (notes?.Length > 180)
            return Error.New($"Notes max length 180 characters.");

        if (charityName.HasHtmlTags() || notes.HasHtmlTags())
        {
            return Error.New(MdpConstants.InputContainingHTMLTagError);
        }

        return new BeneficiaryDetails(charityName, charityNumber.Value, lumpSumPercentage, notes);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Relationship;
        yield return Forenames;
        yield return Surname;
        yield return DateOfBirth;
        yield return CharityName;
        yield return CharityNumber;
        yield return LumpSumPercentage;
        yield return PensionPercentage;
        yield return Notes;
    }
}