using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string username = "user";
        string password = "pass";
        byte[] key = Encoding.UTF8.GetBytes("16ByteSecretKey!");
        byte[] iv = Encoding.UTF8.GetBytes("16ByteInitVector");

        string encryptedUsername = Encrypt(username, key, iv);
        string encryptedPassword = Encrypt(password, key, iv);

        File.WriteAllLines("credentials.txt", new[] { encryptedUsername, encryptedPassword });
        Console.WriteLine("Đã tạo credentials.txt với mã hóa AES.");
    }

    static string Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}