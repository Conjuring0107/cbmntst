using System;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace WaterLevelMonitor
{
    public class SettingsHandler
    {
        private readonly UIHandler ui;
        private readonly Form1 form;

        public SettingsHandler(UIHandler uiHandler, Form1 form)
        {
            ui = uiHandler;
            this.form = form;
        }

        public void LoadSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "config", "settings.txt");
                if (File.Exists(settingsPath))
                {
                    var lines = File.ReadAllLines(settingsPath);
                    var filteredLines = Array.FindAll(lines, line => !line.StartsWith("#"));
                    ui.UpperLimitBox.Text = float.Parse(filteredLines[0], CultureInfo.InvariantCulture).ToString("F3", CultureInfo.InvariantCulture);
                    ui.LowerLimitBox.Text = float.Parse(filteredLines[1], CultureInfo.InvariantCulture).ToString("F3", CultureInfo.InvariantCulture);
                    ui.ToggleUpperButton.Text = int.Parse(filteredLines[2]) == 1 ? "Bật" : "Tắt";
                    ui.ToggleLowerButton.Text = int.Parse(filteredLines[3]) == 1 ? "Bật" : "Tắt";
                    form.ThemeHandler.IsLightTheme = int.Parse(filteredLines[4]) == 1;
                    ui.LimitsLabel.Text = $"Ngưỡng đặt: {ui.LowerLimitBox.Text} - {ui.UpperLimitBox.Text} m";

                    ui.Form.Size = new System.Drawing.Size(405, 520);

                    form.ThemeHandler.UpdateToggleButtonColor(ui.ToggleUpperButton);
                    form.ThemeHandler.UpdateToggleButtonColor(ui.ToggleLowerButton);
                    form.ThemeHandler.ApplyTheme();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể đọc file config/settings.txt: {ex.Message}");
            }
        }

        public void SaveSettings(float upper, float lower, int disconnectThreshold)
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "config", "settings.txt");
                File.WriteAllText(settingsPath,
                    $"# upper_limit\n{upper.ToString("F3", CultureInfo.InvariantCulture)}\n" +
                    $"# lower_limit\n{lower.ToString("F3", CultureInfo.InvariantCulture)}\n" +
                    $"# check_upper\n{(ui.ToggleUpperButton.Text == "Bật" ? 1 : 0)}\n" +
                    $"# check_lower\n{(ui.ToggleLowerButton.Text == "Bật" ? 1 : 0)}\n" +
                    $"# theme\n{(form.ThemeHandler.IsLightTheme ? 1 : 0)}\n" +
                    $"# disconnect_threshold\n{disconnectThreshold.ToString(CultureInfo.InvariantCulture)}");
            }
            catch (Exception)
            {
                Console.WriteLine("Không thể lưu file config/settings.txt");
            }
        }

        public void SaveTheme(bool isLightTheme, int disconnectThreshold)
        {
            try
            {
                string settingsPath = Path.Combine(Application.StartupPath, "config", "settings.txt");
                string upperText = ui.UpperLimitBox.Text.Replace(",", ".");
                string lowerText = ui.LowerLimitBox.Text.Replace(",", ".");
                float upper = float.Parse(upperText, CultureInfo.InvariantCulture);
                float lower = float.Parse(lowerText, CultureInfo.InvariantCulture);
                File.WriteAllText(settingsPath,
                    $"# upper_limit\n{upper.ToString("F3", CultureInfo.InvariantCulture)}\n" +
                    $"# lower_limit\n{lower.ToString("F3", CultureInfo.InvariantCulture)}\n" +
                    $"# check_upper\n{(ui.ToggleUpperButton.Text == "Bật" ? 1 : 0)}\n" +
                    $"# check_lower\n{(ui.ToggleLowerButton.Text == "Bật" ? 1 : 0)}\n" +
                    $"# theme\n{(isLightTheme ? 1 : 0)}\n" +
                    $"# disconnect_threshold\n{disconnectThreshold.ToString(CultureInfo.InvariantCulture)}");
            }
            catch (Exception)
            {
                Console.WriteLine("Không thể lưu file config/settings.txt");
            }
        }
    }
}