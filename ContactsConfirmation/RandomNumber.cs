using System.Security.Cryptography;

namespace WTW.MdpService.ContactsConfirmation;

public static class RandomNumber
{
    public static string Get()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}