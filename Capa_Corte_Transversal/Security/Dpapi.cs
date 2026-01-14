using System;
using System.Security.Cryptography;
using System.Text;

public static class Dpapi
{
    public static string Protect(string plain)
    {
        if (string.IsNullOrEmpty(plain)) return "";
        byte[] data = Encoding.UTF8.GetBytes(plain);
        byte[] enc = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(enc);
    }

    public static string Unprotect(string protectedBase64)
    {
        if (string.IsNullOrEmpty(protectedBase64)) return "";
        byte[] enc = Convert.FromBase64String(protectedBase64);
        byte[] data = ProtectedData.Unprotect(enc, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }
}
