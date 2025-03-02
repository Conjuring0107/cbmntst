using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class CredentialManager
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("16ByteSecretKey!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("16ByteInitVector");
        private readonly string credentialsPath;

        public string Username { get; private set; }
        public string Password { get; private set; }

        public CredentialManager()
        {
            credentialsPath = Path.Combine(Application.StartupPath, "config", "credentials.txt");
            LoadCredentials();
        }

        private void LoadCredentials()
        {
            try
            {
                if (File.Exists(credentialsPath))
                {
                    var lines = File.ReadAllLines(credentialsPath);
                    Username = Decrypt(lines[0]);
                    Password = Decrypt(lines[1]);
                }
                else
                {
                    throw new Exception("Thiếu file credentials.txt trong thư mục config.");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Không thể đọc file credentials.txt: {ex.Message}");
                throw;
            }
        }

        private string Decrypt(string encryptedText)
        {
            byte[] cipherBytes = Convert.FromBase64String(encryptedText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}