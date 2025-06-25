using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace WTW.MdpService.BereavementContactsConfirmation;

public static class EmailSecurity
{
    public static string Hash(string email)
    {
        using var sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(email));
        var builder = new StringBuilder();
        foreach (byte t in bytes)
            builder.Append(t.ToString("x2"));
        
        return builder.ToString();
    }
    
    public static bool CheckHash(string hashedEmail, string email)
    {
        return Hash(email) == hashedEmail;
    }
}