using System;
using System.IO;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public static class LogManager
    {
        public static void LogNetworkStatus(DateTime? startTime, DateTime? endTime)
        {
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string logFolder = Path.Combine(Application.StartupPath, "config", "logs", currentDate);
            Directory.CreateDirectory(logFolder);
            string logFile = Path.Combine(logFolder, "network_log.txt");

            using (StreamWriter sw = File.AppendText(logFile))
            {
                if (startTime.HasValue)
                    sw.WriteLine($"Mất kết nối mạng: {startTime.Value.ToString("HH:mm:ss dd-MM-yyyy")}");
                if (endTime.HasValue)
                    sw.WriteLine($"Kết nối lại: {endTime.Value.ToString("HH:mm:ss dd-MM-yyyy")}\n");
            }
        }

        public static void LogError(string message)
        {
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string logFolder = Path.Combine(Application.StartupPath, "config", "logs", currentDate);
            Directory.CreateDirectory(logFolder);
            string logFile = Path.Combine(logFolder, "error_log.txt");

            if (File.Exists(logFile) && new FileInfo(logFile).Length > 1024 * 1024)
            {
                File.WriteAllText(logFile, "");
            }

            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy")}: {message}");
            }
        }

        public static void LogInfo(string message)
        {
            string currentDate = DateTime.Now.ToString("dd-MM-yyyy");
            string logFolder = Path.Combine(Application.StartupPath, "config", "logs", currentDate);
            Directory.CreateDirectory(logFolder);
            string logFile = Path.Combine(logFolder, "info_log.txt");

            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy")}: {message}");
            }
        }
    }
}