using System;
using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members;

public class PersonalDetails : ValueObject
{
    protected PersonalDetails() { }

    private PersonalDetails(string title, string gender, string forenames, string surname, DateTimeOffset? dateOfBirth)
    {
        Title = title;
        Gender = gender;
        Forenames = forenames;
        Surname = surname;
        DateOfBirth = dateOfBirth;
    }

    public string Title { get; }
    public string Gender { get; }
    public string Forenames { get; }
    public string Surname { get; }
    public DateTimeOffset? DateOfBirth { get; }

    public static Either<Error, PersonalDetails> Create(string title,
        string gender,
        string forenames,
        string surname,
        DateTimeOffset? dateOfBirth)
    {
        if (title != null && title.Length > 10)
            return Error.New("Title must be less than 10 characters lenght.");

        if (gender?.ToUpper() != "M" && gender?.ToUpper() != "F")
            return Error.New("Invalid gender provided: It must be F for female or M for male.");

        if (forenames?.Length > 32)
            return Error.New("Forenames must be less than 32 characters lenght.");

        if (surname?.Length > 20)
            return Error.New("Surname must be less than 20 characters lenght.");

        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTimeOffset.UtcNow)
            return Error.New("Invalid date of birth: Future date unable to be date of birth.");

        return new PersonalDetails(title, gender, forenames, surname, dateOfBirth);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Title;
        yield return Gender;
        yield return Forenames;
        yield return Surname;
        yield return DateOfBirth;
    }
}