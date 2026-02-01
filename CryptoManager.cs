using System.Security.Cryptography;
using System.Text;

namespace PosnaiSQLauncher;

public static class CryptoManager
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("Tyfelka1408Tyfelka1408Tyfelka140"); 
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("MamaTyyfelka2210"); 

    public static string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var buffer = Encoding.UTF8.GetBytes(text);

        var encrypted = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Convert.ToBase64String(encrypted);
    }
    
    public static string SafeDecrypt(string cipher)
    {
        if (string.IsNullOrWhiteSpace(cipher)) return cipher;

        try
        {
            Convert.FromBase64String(cipher);
            return Decrypt(cipher);
        }
        catch
        {
            return cipher;
        }
    }


    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var buffer = Convert.FromBase64String(cipherText);

        var decrypted = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(decrypted);
    }
}