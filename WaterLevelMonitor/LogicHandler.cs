using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class LogicHandler
    {
        private readonly UIHandler ui;
        private readonly SettingsHandler settings;
        private readonly ThemeHandler themeHandler;
        private readonly CredentialManager credentials;
        private readonly MonitoringService monitoringService;
        private int disconnectThreshold = 60;
        private float updatedUpperLimit = 0;
        private float updatedLowerLimit = 0;

        public LogicHandler(UIHandler uiHandler, SettingsHandler settingsHandler, ThemeHandler themeHandler)
        {
            ui = uiHandler;
            settings = settingsHandler;
            this.themeHandler = themeHandler;
            credentials = new CredentialManager();
            monitoringService = new MonitoringService(uiHandler, credentials, themeHandler, this);
            LoadDisconnectThreshold();
        }

        private void LoadDisconnectThreshold()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "config", "settings.txt");
                if (File.Exists(settingsPath))
                {
                    var lines = File.ReadAllLines(settingsPath);
                    var filteredLines = Array.FindAll(lines, line => !line.StartsWith("#"));
                    if (filteredLines.Length >= 6)
                    {
                        disconnectThreshold = int.Parse(filteredLines[5], CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError($"Không thể đọc DISCONNECT_THRESHOLD từ settings.txt: {ex.Message}");
                disconnectThreshold = 60;
            }
        }

        public int GetDisconnectThreshold()
        {
            return disconnectThreshold;
        }

        public void SetDisconnectThreshold(int value)
        {
            disconnectThreshold = value;
            monitoringService.SetDisconnectThreshold(value);
        }

        public void SetTogglingLimits(bool value)
        {
            monitoringService.SetTogglingLimits(value);
        }

        public async Task StartMonitoring()
        {
            await monitoringService.StartMonitoring();
        }

        public void StopMonitoring()
        {
            monitoringService.StopMonitoring();
        }

        public async Task ResetMonitoring()
        {
            await monitoringService.ResetMonitoring();
        }

        public void UpdateLimits(float upper, float lower)
        {
            try
            {
                updatedUpperLimit = upper;
                updatedLowerLimit = lower;

                ui.Form.Invoke(new MethodInvoker(() =>
                {
                    ui.UpperLimitBox.Text = upper.ToString("F3", CultureInfo.InvariantCulture);
                    ui.LowerLimitBox.Text = lower.ToString("F3", CultureInfo.InvariantCulture);
                    ui.LimitsLabel.Text = $"Ngưỡng đặt: {lower.ToString("F3", CultureInfo.InvariantCulture)} - {upper.ToString("F3", CultureInfo.InvariantCulture)} m";
                }));
                settings.SaveSettings(upper, lower, disconnectThreshold);
            }
            catch (Exception ex)
            {
                ui.Form.Invoke(new MethodInvoker(() =>
                {
                    new AlertForm($"Lỗi: {ex.Message}", "Lỗi", themeHandler).ShowDialog(ui.Form);
                }));
            }
        }

        public float GetUpdatedUpperLimit()
        {
            return updatedUpperLimit;
        }

        public float GetUpdatedLowerLimit()
        {
            return updatedLowerLimit;
        }
    }
}