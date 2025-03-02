using System;
using System.IO;
using Serilog;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            // Cấu hình log cho Debug mode: Ghi tất cả các cấp độ (Info, Error, Network) vào file
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(Application.StartupPath, "config", "logs", "debug_log.txt"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();
#else
            // Cấu hình log cho Release mode: Chỉ ghi Network, tự xóa khi vượt 5MB
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine(Application.StartupPath, "config", "logs", "release_log.txt"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [Network] {Message}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5 * 1024 * 1024, // Giới hạn 5MB
                    retainedFileCountLimit: 3)
                .CreateLogger();
            CleanupLogsIfExceedsSize();
#endif

            try
            {
                Log.Information("Starting application...");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Application crashed");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void CleanupLogsIfExceedsSize()
        {
            string logFolder = Path.Combine(Application.StartupPath, "config", "logs");
            if (!Directory.Exists(logFolder)) return;

            long totalSize = 0;
            foreach (string file in Directory.GetFiles(logFolder, "*.txt"))
            {
                FileInfo fi = new FileInfo(file);
                totalSize += fi.Length;
            }

            if (totalSize > 5 * 1024 * 1024) // 5MB
            {
                Directory.Delete(logFolder, true);
                Directory.CreateDirectory(logFolder);
                Log.Information("Log folder cleaned due to size exceeding 5MB.");
            }
        }
    }
}