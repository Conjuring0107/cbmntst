using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class SettingsForm : Form
    {
        private readonly SettingsHandler settingsHandler;
        private readonly ThemeHandler themeHandler;
        private readonly LogicHandler logicHandler;
        private readonly Form1 mainForm;

        private RadioButton lightTheme;
        private RadioButton darkTheme;
        private TextBox disconnectThresholdBox;

        public SettingsForm(SettingsHandler settingsHandler, ThemeHandler themeHandler, LogicHandler logicHandler, Form1 mainForm)
        {
            this.settingsHandler = settingsHandler ?? throw new ArgumentNullException(nameof(settingsHandler), "SettingsHandler không thể là null.");
            this.themeHandler = themeHandler ?? throw new ArgumentNullException(nameof(themeHandler), "ThemeHandler không thể là null.");
            this.logicHandler = logicHandler ?? throw new ArgumentNullException(nameof(logicHandler), "LogicHandler không thể là null.");
            this.mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm), "Form1 không thể là null.");
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Settings";
            Size = new Size(250, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#FFFFFF") : ColorTranslator.FromHtml("#202020");

            Label themeLabel = new Label
            {
                Text = "Chọn Theme:",
                Size = new Size(100, 20),
                Location = new Point(20, 20),
                ForeColor = themeHandler.IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3")
            };
            lightTheme = new RadioButton
            {
                Text = "Light",
                Size = new Size(60, 20),
                Location = new Point(20, 50),
                Checked = themeHandler.IsLightTheme,
                ForeColor = themeHandler.IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3")
            };
            darkTheme = new RadioButton
            {
                Text = "Dark",
                Size = new Size(60, 20),
                Location = new Point(80, 50),
                Checked = !themeHandler.IsLightTheme,
                ForeColor = themeHandler.IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3")
            };

            Label disconnectLabel = new Label
            {
                Text = "Thời gian chờ (s):",
                Size = new Size(120, 20),
                Location = new Point(20, 80),
                ForeColor = themeHandler.IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3")
            };
            disconnectThresholdBox = new TextBox
            {
                Text = logicHandler?.GetDisconnectThreshold().ToString(CultureInfo.InvariantCulture) ?? "60",
                Size = new Size(100, 20),
                Location = new Point(20, 110),
                Font = new Font("Arial", 10)
            };

            Button applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(60, 25),
                Location = new Point(90, 180),
                BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                ForeColor = Color.White
            };
            applyButton.Click += ApplyButton_Click;

            Controls.Add(themeLabel);
            Controls.Add(lightTheme);
            Controls.Add(darkTheme);
            Controls.Add(disconnectLabel);
            Controls.Add(disconnectThresholdBox);
            Controls.Add(applyButton);

            FormClosed += SettingsForm_FormClosed;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            try
            {
                themeHandler.IsLightTheme = lightTheme.Checked;
                themeHandler.ApplyTheme(this);
                BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#FFFFFF") : ColorTranslator.FromHtml("#202020");
                themeHandler.UpdateAllControlsTheme(this);

                if (mainForm != null && !mainForm.IsDisposed)
                {
                    mainForm.UpdateGlobalTheme(themeHandler.IsLightTheme);
                }

                int disconnectThreshold = int.Parse(disconnectThresholdBox.Text, CultureInfo.InvariantCulture);
                if (disconnectThreshold < 10)
                    throw new Exception("Thời gian chờ phải lớn hơn 10 giây!");

                settingsHandler.SaveTheme(themeHandler.IsLightTheme, disconnectThreshold);
                if (logicHandler != null)
                    logicHandler.SetDisconnectThreshold(disconnectThreshold);

                Form1.Log.Information("Settings applied successfully - Theme: {Theme}, DisconnectThreshold: {Threshold}",
                    themeHandler.IsLightTheme ? "Light" : "Dark", disconnectThreshold);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Form1.Log.Error($"Lỗi khi lưu settings: {ex.Message}");
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
            }
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.Log.Debug("Settings form đã đóng hoàn toàn.");
            Dispose();
        }
    }
}