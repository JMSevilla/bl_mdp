using System.Collections.Generic;
using System.Text.RegularExpressions;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain;

public class Email : ValueObject
{
    protected Email() { }

    private Email(string address)
    {
        Address = address;
    }

    public string Address { get; }

    public static Either<Error, Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > 50)
            return Error.New("Email is required. Up to 50 characters length.");

        email = email.Trim().ToLower();
        if (!Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
            return Error.New("Invalid email address format.");

        return new Email(email);
    }

    public static Email Empty()
    {
        return new Email();
    }

    public Email Clone()
    {
        return new Email(Address);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Address;
    }

    public static implicit operator string(Email email)
    {
        return email.Address;
    }
}