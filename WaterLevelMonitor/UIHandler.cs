using Serilog;
using System;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class UIHandler
    {
        private readonly Form1 form;
        private readonly ThemeHandler themeHandler;
        private LogicHandler logicHandler;
        private readonly MainFormUI mainFormUI;
        private static SettingsForm settingsForm;
        private static TutorialForm tutorialForm;

        public UIHandler(Form1 form, ThemeHandler themeHandler, LogicHandler logicHandler)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form), "Form không thể là null.");
            this.themeHandler = themeHandler ?? throw new ArgumentNullException(nameof(themeHandler), "ThemeHandler không thể là null.");
            this.logicHandler = logicHandler;
            try
            {
                mainFormUI = CreateMainFormUI();
                Form1.Log.Information("UIHandler đã khởi tạo MainFormUI thành công: {IsNotNull}", mainFormUI != null);
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi khởi tạo MainFormUI trong UIHandler");
                throw new NullReferenceException($"Không thể khởi tạo MainFormUI: {ex.Message}", ex);
            }
            SetupUI();
        }

        private MainFormUI CreateMainFormUI()
        {
            try
            {
                MainFormUI ui = new MainFormUI(form, themeHandler);
                return ui;
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi khởi tạo MainFormUI");
                throw new NullReferenceException($"Lỗi khi khởi tạo MainFormUI: {ex.Message}", ex);
            }
        }

        public void SetLogicHandler(LogicHandler logicHandler)
        {
            this.logicHandler = logicHandler ?? throw new ArgumentNullException(nameof(logicHandler), "LogicHandler không thể là null.");
        }

        public void SetupUI()
        {
            if (mainFormUI == null) throw new NullReferenceException("MainFormUI chưa được khởi tạo.");
            Form1.Log.Information("Đang thiết lập UI trong UIHandler với mainFormUI: {IsNotNull}", mainFormUI != null);
            mainFormUI.TopMostButton.Click += form.TopMostButton_Click;

            mainFormUI.UpperLimitBox.TextChanged += (s, e) => form.UpdateLimitButtonsState();
            mainFormUI.LowerLimitBox.TextChanged += (s, e) => form.UpdateLimitButtonsState();
        }

        public void OpenSettingsForm()
        {
            Log.Debug("Bắt đầu mở Settings form.");
            if (settingsForm == null || settingsForm.IsDisposed)
            {
                settingsForm = new SettingsForm(form.SettingsHandler, themeHandler, logicHandler, form);
                settingsForm.FormClosed += SettingsForm_FormClosed;
                Log.Debug("Hiển thị Settings form bằng ShowDialog.");
                settingsForm.ShowDialog(form);
                settingsForm = null;
                Log.Information("Settings form đã hiển thị và đóng hoàn toàn.");
            }
            else
            {
                Log.Debug("Settings form đã tồn tại, đưa lên trước.");
                settingsForm.BringToFront();
            }
            form.Refresh();
        }

        public void TopMostButton_Click(object sender, EventArgs e)
        {
            Form1.Log.Debug("Bắt đầu toggle TopMost - Trạng thái hiện tại: {IsTopMost}", form.TopMost);
            form.TopMost = !form.TopMost;
            mainFormUI.TopMostButton.Text = form.TopMost ? "Unpin" : "Pin";
            mainFormUI.TopMostButton.Refresh();
            Form1.Log.Information("TopMost status toggled to: {IsTopMost}", form.TopMost);
        }

        public void TutorialButton_Click(object sender, EventArgs e)
        {
            Log.Debug("Bắt đầu mở Tutorial form.");
            if (tutorialForm == null || tutorialForm.IsDisposed)
            {
                tutorialForm = new TutorialForm(themeHandler);
                tutorialForm.FormClosed += TutorialForm_FormClosed;
                Log.Debug("Hiển thị Tutorial form bằng ShowDialog.");
                tutorialForm.ShowDialog(form);
                tutorialForm = null;
                Log.Information("Tutorial form đã hiển thị và đóng hoàn toàn.");
            }
            else
            {
                Log.Debug("Tutorial form đã tồn tại, đưa lên trước.");
                tutorialForm.BringToFront();
            }
            form.Refresh();
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.Debug("SettingsForm_FormClosed được gọi.");
            settingsForm = null;
        }

        private void TutorialForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.Debug("TutorialForm_FormClosed được gọi.");
            tutorialForm = null;
        }

        public void ToggleUpper()
        {
            mainFormUI.ToggleUpper();
            Form1.Log.Debug("ToggleUpper called - State: {ToggleUpperState}", mainFormUI.ToggleUpperButton.Text);
        }

        public void ToggleLower()
        {
            mainFormUI.ToggleLower();
            Form1.Log.Debug("ToggleLower called - State: {ToggleLowerState}", mainFormUI.ToggleLowerButton.Text);
        }

        public Label LevelLabel => mainFormUI.LevelLabel ?? throw new NullReferenceException("LevelLabel chưa được khởi tạo.");
        public Label StatusLabel => mainFormUI.StatusLabel ?? throw new NullReferenceException("StatusLabel chưa được khởi tạo.");
        public TextBox UpperLimitBox => mainFormUI.UpperLimitBox ?? throw new NullReferenceException("UpperLimitBox chưa được khởi tạo.");
        public TextBox LowerLimitBox => mainFormUI.LowerLimitBox ?? throw new NullReferenceException("LowerLimitBox chưa được khởi tạo.");
        public Button ToggleUpperButton => mainFormUI.ToggleUpperButton ?? throw new NullReferenceException("ToggleUpperButton chưa được khởi tạo.");
        public Button ToggleLowerButton => mainFormUI.ToggleLowerButton ?? throw new NullReferenceException("ToggleLowerButton chưa được khởi tạo.");
        public Button StartButton => mainFormUI.StartButton ?? throw new NullReferenceException("StartButton chưa được khởi tạo.");
        public Button StopButton => mainFormUI.StopButton ?? throw new NullReferenceException("StopButton chưa được khởi tạo.");
        public Button ResetButton => mainFormUI.ResetButton ?? throw new NullReferenceException("ResetButton chưa được khởi tạo.");
        public Button UpdateButton => mainFormUI.UpdateButton ?? throw new NullReferenceException("UpdateButton chưa được khởi tạo.");
        public Button SettingsButton => mainFormUI.SettingsButton ?? throw new NullReferenceException("SettingsButton chưa được khởi tạo.");
        public Button TopMostButton => mainFormUI.TopMostButton ?? throw new NullReferenceException("TopMostButton chưa được khởi tạo.");
        public Button TutorialButton => mainFormUI.TutorialButton ?? throw new NullReferenceException("TutorialButton chưa được khởi tạo.");
        public Label LimitsLabel => mainFormUI.LimitsLabel ?? throw new NullReferenceException("LimitsLabel chưa được khởi tạo.");
        public Form Form => form;
    }
}