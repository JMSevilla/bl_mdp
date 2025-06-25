using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain;

public class Phone : ValueObject
{
    protected Phone()
    {
    }

    private Phone(string code, string number)
    {
        FullNumber = $"{code} {number}";
    }

    private Phone(string fullNumber)
    {
        FullNumber = fullNumber;
    }

    public string FullNumber { get; set; }

    public static Either<Error, Phone> Create(string code, string number)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 4)
            return Error.New("Phone code is required. Up to 4 characters length.");

        if (!Regex.IsMatch(code, @"^(\d{1,3}|\d{1,4})$"))
            return Error.New("Invalid code format.");

        if (string.IsNullOrWhiteSpace(number) || number.Length > 20)
            return Error.New("Phone number is required. Up to 20 characters length.");

        if (!Regex.IsMatch(number, @"^[0-9]*$"))
            return Error.New("Invalid number format. It must contain only numbers.");

        return new Phone(code, number);
    }

    public static Either<Error, Phone> Create(string full)
    {        
        if (string.IsNullOrWhiteSpace(full) || full.Length > 25)
            return Error.New("Phone number is required. Up to 24 characters length.");

        if (!Regex.IsMatch(full, @"^[0-9 ]+"))
            return Error.New("Invalid phone number format.");


        return new Phone(full);
    }

    public static Phone Empty()
    {
        return new Phone();
    }

    public Phone Clone()
    {
        return new Phone(FullNumber);
    }

    public string Code()
    {
        return FullNumber?.Split(" ").FirstOrDefault();
    }

    public string Number()
    {
        return FullNumber?.Split(" ").Skip(1).FirstOrDefault();
    }

    protected override IEnumerable<object> Parts()
    {
        yield return FullNumber;
    }

    public static implicit operator string(Phone phone)
    {
        return phone.FullNumber;
    }
}