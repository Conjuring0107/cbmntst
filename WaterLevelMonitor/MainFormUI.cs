using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WaterLevelMonitor;

namespace WaterLevelMonitor
{
    public class MainFormUI
    {
        private readonly Form1 form;
        private readonly ThemeHandler themeHandler;
        private readonly int defaultWidth = 405;
        private readonly int defaultHeight = 520;
        private Panel statusFrame;
        private Panel limitsFrame;
        private Panel buttonFrame;
        private Label buildLabel;

        public Label LevelLabel { get; private set; }
        public Label StatusLabel { get; private set; }
        public TextBox UpperLimitBox { get; private set; }
        public TextBox LowerLimitBox { get; private set; }
        public Button ToggleUpperButton { get; private set; }
        public Button ToggleLowerButton { get; private set; }
        public Button StartButton { get; private set; }
        public Button StopButton { get; private set; }
        public Button ResetButton { get; private set; }
        public Button UpdateButton { get; private set; }
        public Button SettingsButton { get; private set; }
        public Button TopMostButton { get; private set; }
        public Button TutorialButton { get; private set; }
        public Label LimitsLabel { get; private set; }

        public MainFormUI(Form1 form, ThemeHandler themeHandler)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form), "Form không thể là null.");
            this.themeHandler = themeHandler ?? throw new ArgumentNullException(nameof(themeHandler), "ThemeHandler không thể là null.");
            try
            {
                SetupUI();
                Form1.Log.Information("MainFormUI đã được khởi tạo thành công.");
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi khởi tạo MainFormUI");
                throw new NullReferenceException($"Không thể khởi tạo MainFormUI: {ex.Message}", ex);
            }
        }

        private void SetupUI()
        {
            if (form == null) throw new NullReferenceException("Form không được khởi tạo.");
            Form1.Log.Information("Bắt đầu thiết lập UI trong MainFormUI.");
            form.Text = "Theo dõi mực nước - Thượng Sơn Tây";
            form.Size = new Size(defaultWidth, defaultHeight);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            try
            {
                string iconPath = Path.Combine(Application.StartupPath, "config", "app_icon.ico");
                if (File.Exists(iconPath))
                {
                    form.Icon = new Icon(iconPath);
                    Form1.Log.Information("Đã thiết lập icon cho form thành công.");
                }
                else
                {
                    Form1.Log.Warning("Không tìm thấy file app_icon.ico, bỏ qua thiết lập icon.");
                }
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi thiết lập icon");
            }

            form.Resize += (s, e) => AdjustLayout();

            try
            {
                statusFrame = new Panel { Size = new Size(380, 80), BorderStyle = BorderStyle.FixedSingle };
                statusFrame.Location = new Point((defaultWidth - 380) / 2, 35);
                LevelLabel = new Label { Text = "Mực nước: 0.000 m", Size = new Size(300, 25), Font = new Font("Arial", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
                LevelLabel.Location = new Point(40, 10);
                StatusLabel = new Label { Text = "Trạng thái: Đang chờ khởi động, ấn start", Size = new Size(300, 20), Font = new Font("Arial", 10), TextAlign = ContentAlignment.MiddleCenter };
                StatusLabel.Location = new Point(40, 40);
                statusFrame.Controls.AddRange(new Control[] { LevelLabel, StatusLabel });

                limitsFrame = new Panel { Size = new Size(380, 200), BorderStyle = BorderStyle.FixedSingle };
                limitsFrame.Location = new Point((defaultWidth - 380) / 2, 125);
                Label lowerLabel = new Label { Text = "Ngưỡng thấp (m):", Size = new Size(120, 20), Font = new Font("Arial", 10), TextAlign = ContentAlignment.MiddleLeft };
                lowerLabel.Location = new Point(20, 25);
                LowerLimitBox = new TextBox { Text = "246.000", Size = new Size(100, 20), Font = new Font("Arial", 10) };
                LowerLimitBox.Location = new Point(150, 25);
                ToggleLowerButton = new Button { Text = "Bật", Size = new Size(60, 25), Font = new Font("Arial", 8) };
                ToggleLowerButton.Location = new Point(LowerLimitBox.Right + 10, 25);

                Label upperLabel = new Label { Text = "Ngưỡng cao (m):", Size = new Size(120, 20), Font = new Font("Arial", 10), TextAlign = ContentAlignment.MiddleLeft };
                upperLabel.Location = new Point(20, 65);
                UpperLimitBox = new TextBox { Text = "248.000", Size = new Size(100, 20), Font = new Font("Arial", 10) };
                UpperLimitBox.Location = new Point(150, 65);
                ToggleUpperButton = new Button { Text = "Bật", Size = new Size(60, 25), Font = new Font("Arial", 8) };
                ToggleUpperButton.Location = new Point(UpperLimitBox.Right + 10, 65);

                UpdateButton = new Button { Text = "Cập nhật", Size = new Size(100, 30), Font = new Font("Arial", 10, FontStyle.Bold) };
                UpdateButton.Location = new Point(140, 105);
                LimitsLabel = new Label
                {
                    Text = "Ngưỡng đặt: 246.000 - 248.000 m",
                    Size = new Size(320, 25),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = themeHandler.IsLightTheme ? Color.Black : Color.White,
                    Location = new Point(30, 150)
                };
                limitsFrame.Controls.AddRange(new Control[] { UpperLimitBox, LowerLimitBox, ToggleUpperButton, ToggleLowerButton, upperLabel, lowerLabel, UpdateButton, LimitsLabel });

                buttonFrame = new Panel { Size = new Size(380, 60), BorderStyle = BorderStyle.FixedSingle };
                buttonFrame.Location = new Point((defaultWidth - 380) / 2, 335);
                StartButton = new Button { Text = "Start", Size = new Size(100, 30), Font = new Font("Arial", 10, FontStyle.Bold) };
                StopButton = new Button { Text = "Stop", Size = new Size(100, 30), Enabled = false, Font = new Font("Arial", 10, FontStyle.Bold) };
                ResetButton = new Button { Text = "Reset", Size = new Size(100, 30), Enabled = false, Font = new Font("Arial", 10, FontStyle.Bold) };
                int buttonSpacing = 20;
                int totalButtonWidth = 3 * StartButton.Width + 2 * buttonSpacing;
                StartButton.Location = new Point((buttonFrame.Width - totalButtonWidth) / 2, 15);
                StopButton.Location = new Point(StartButton.Right + buttonSpacing, 15);
                ResetButton.Location = new Point(StopButton.Right + buttonSpacing, 15);
                buttonFrame.Controls.AddRange(new Control[] { StartButton, StopButton, ResetButton });

                buildLabel = new Label
                {
                    Text = $"Gia Bảo - TST  |  Build: {DateTime.Now.ToString("dd-MM-yyyy")}\nAll rights reserved ®",
                    Size = new Size(300, 50),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                    Location = new Point((defaultWidth - 300) / 2, 410)
                };
                buildLabel.MouseEnter += (s, e) => buildLabel.ForeColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#005BB5") : ColorTranslator.FromHtml("#3B9EFF");
                buildLabel.MouseLeave += (s, e) => buildLabel.ForeColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4");

                SettingsButton = new Button
                {
                    Text = "Settings",
                    Size = new Size(80, 25),
                    Font = new Font("Arial", 7, FontStyle.Bold),
                    Location = new Point(defaultWidth - 90, 5),
                    BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                    ForeColor = Color.White
                };
                SettingsButton.Click += (s, e) => form.SettingsButton_Click(s, e);

                TopMostButton = new Button
                {
                    Text = "Pin",
                    Size = new Size(80, 25),
                    Font = new Font("Arial", 7, FontStyle.Bold),
                    Location = new Point(defaultWidth - 180, 5),
                    BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                    ForeColor = Color.White
                };
                TopMostButton.Click += (s, e) => form.TopMostButton_Click(s, e);

                TutorialButton = new Button
                {
                    Text = "Tutorial",
                    Size = new Size(80, 25),
                    Font = new Font("Arial", 7, FontStyle.Bold),
                    Location = new Point(defaultWidth - 270, 5),
                    BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                    ForeColor = Color.White
                };
                TutorialButton.Click += (s, e) => form.TutorialButton_Click(s, e);

                form.Controls.AddRange(new Control[] { statusFrame, limitsFrame, buttonFrame, buildLabel, SettingsButton, TopMostButton, TutorialButton });
                Form1.Log.Information("Đã thêm controls vào form thành công.");
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi thêm controls vào form trong SetupUI");
                throw new NullReferenceException($"Lỗi khi thiết lập UI: {ex.Message}", ex);
            }

            AdjustLayout();
            Form1.Log.Information("Hoàn thành thiết lập UI trong MainFormUI.");
        }

        public void ToggleUpper()
        {
            Form1.Log.Debug("Bắt đầu toggle upper limit - State: {CurrentState}", ToggleUpperButton.Text);
            ToggleUpperButton.Text = ToggleUpperButton.Text == "Bật" ? "Tắt" : "Bật";
            themeHandler.UpdateToggleButtonColor(ToggleUpperButton);
            Form1.Log.Information("Upper limit toggled - New State: {NewState}", ToggleUpperButton.Text);
        }

        public void ToggleLower()
        {
            Form1.Log.Debug("Bắt đầu toggle lower limit - State: {CurrentState}", ToggleLowerButton.Text);
            ToggleLowerButton.Text = ToggleLowerButton.Text == "Bật" ? "Tắt" : "Bật";
            themeHandler.UpdateToggleButtonColor(ToggleLowerButton);
            Form1.Log.Information("Lower limit toggled - New State: {NewState}", ToggleLowerButton.Text);
        }

        private void AdjustLayout()
        {
            int formWidth = form.ClientSize.Width;
            int panelWidth = Math.Max(380, formWidth - 40);

            statusFrame.Size = new Size(panelWidth, 80);
            statusFrame.Location = new Point((formWidth - panelWidth) / 2, 35);
            LevelLabel.Size = new Size(panelWidth - 80, 25);
            LevelLabel.Location = new Point(40, 10);
            StatusLabel.Size = new Size(panelWidth - 80, 20);
            StatusLabel.Location = new Point(40, 40);

            limitsFrame.Size = new Size(panelWidth, 200);
            limitsFrame.Location = new Point((formWidth - panelWidth) / 2, 125);
            UpperLimitBox.Location = new Point((panelWidth - 270) / 2 + 120, 65);
            LowerLimitBox.Location = new Point((panelWidth - 270) / 2 + 120, 25);
            ToggleUpperButton.Location = new Point(UpperLimitBox.Right + 10, 65);
            ToggleLowerButton.Location = new Point(LowerLimitBox.Right + 10, 25);
            UpdateButton.Location = new Point((panelWidth - 100) / 2, 105);
            LimitsLabel.Size = new Size(panelWidth - 60, 25);
            LimitsLabel.Location = new Point(30, 150);

            buttonFrame.Size = new Size(panelWidth, 60);
            buttonFrame.Location = new Point((formWidth - panelWidth) / 2, 335);
            int totalButtonWidth = 3 * StartButton.Width + 40;
            StartButton.Location = new Point((buttonFrame.Width - totalButtonWidth) / 2, 15);
            StopButton.Location = new Point(StartButton.Right + 20, 15);
            ResetButton.Location = new Point(StopButton.Right + 20, 15);

            buildLabel.Size = new Size(panelWidth - 80, 50);
            buildLabel.Location = new Point((formWidth - panelWidth) / 2 + 40, 410);
            SettingsButton.Location = new Point(formWidth - 90, 5);
            TopMostButton.Location = new Point(formWidth - 180, 5);
            TutorialButton.Location = new Point(formWidth - 270, 5);
        }
    }
}