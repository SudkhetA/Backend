using System.Security.Cryptography;
using System.Text;

namespace Backend.Utilities.Helper;

public class EncryptedHelper(IConfiguration configuration)
{
    private const int NonceSize = 96 / 8;
    private const int TagSize = 128 / 8;

    public string EncryptAes(string text)
    {
        var key = configuration.GetSection("Encryption:PrivateKeyPassword").Value;
        if (!string.IsNullOrEmpty(key))
        {
            return EncryptAes(key, text);
        }

        throw new InvalidOperationException("Private key is null or empty.");
    }

    public string EncryptAes(string key, string text)
    {
        try
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            var plaintextBytes = Encoding.UTF8.GetBytes(text);

            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);
            var cipherText = new byte[plaintextBytes.Length];
            var tag = new byte[TagSize];

            using var aesGcm = new AesGcm(hash, tag.Length);

            aesGcm.Encrypt(nonce, plaintextBytes, cipherText, tag);
            var result = new byte[nonce.Length + cipherText.Length + tag.Length];
            Array.Copy(nonce, 0, result, 0, nonce.Length);
            Array.Copy(cipherText, 0, result, nonce.Length, cipherText.Length);
            Array.Copy(tag, 0, result, nonce.Length + cipherText.Length, tag.Length);

            return Convert.ToBase64String(result);
        }
        catch
        {
            return "";
        }
    }

    public string DecryptAes(string text)
    {
        var key = configuration.GetSection("Encryption:PrivateKeyPassword").Value;
        if (!string.IsNullOrEmpty(key))
        {
            return DecryptAes(key, text);
        }

        throw new InvalidOperationException("Private key is null or empty.");
    }

    public string DecryptAes(string key, string text)
    {
        try
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            var encryptedBytes = Convert.FromBase64String(text);

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var cipherText = new byte[encryptedBytes.Length - nonce.Length - tag.Length];

            Array.Copy(encryptedBytes, nonce, nonce.Length);
            Array.Copy(encryptedBytes, nonce.Length, cipherText, 0, cipherText.Length);
            Array.Copy(encryptedBytes, nonce.Length + cipherText.Length, tag, 0, tag.Length);

            using var aesGcm = new AesGcm(hash, tag.Length);

            var plaintext = new byte[cipherText.Length];
            aesGcm.Decrypt(nonce, cipherText, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch
        {
            return "";
        }
    }

    public bool CompareEncryptedTextWithPlainText(string encryptedText, string plainText)
    {
        var key = configuration.GetSection("Encryption:PrivateKeyPassword").Value;
        if (!string.IsNullOrEmpty(key))
        {
            return CompareEncryptedTextWithPlainText(key, encryptedText, plainText);
        }

        throw new InvalidOperationException("Private key is null or empty.");
    }

    public bool CompareEncryptedTextWithPlainText(string key, string encryptedText, string plainText)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherText = new byte[plaintextBytes.Length];

        Array.Copy(encryptedBytes, nonce, nonce.Length);

        using var aesGcm = new AesGcm(hash, tag.Length);
        aesGcm.Encrypt(nonce, plaintextBytes, cipherText, tag);

        var result = new byte[nonce.Length + cipherText.Length + tag.Length];
        Array.Copy(nonce, 0, result, 0, nonce.Length);
        Array.Copy(cipherText, 0, result, nonce.Length, cipherText.Length);
        Array.Copy(tag, 0, result, nonce.Length + cipherText.Length, tag.Length);

        return encryptedBytes.SequenceEqual(result);
    }
}
