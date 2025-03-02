using System;
using System.Windows.Forms;
using System.Globalization;
using WaterLevelMonitor;
using Serilog;

namespace WaterLevelMonitor
{
    public partial class Form1 : Form
    {
        public static ILogger Log => Serilog.Log.Logger;

        private readonly UIHandler uiHandler;
        private readonly LogicHandler logicHandler;
        private readonly ThemeHandler themeHandler;
        private readonly SettingsHandler settingsHandler;

        public Form1()
        {
            InitializeComponent();
            Log.Information("Bắt đầu khởi tạo Form1.");
            themeHandler = new ThemeHandler(this) ?? throw new NullReferenceException("Không thể khởi tạo ThemeHandler.");
            Log.Information("ThemeHandler đã được khởi tạo: {IsNotNull}", themeHandler != null);
            uiHandler = CreateUIHandler();
            if (uiHandler == null)
            {
                Log.Error("UIHandler không thể khởi tạo, ném exception.");
                throw new NullReferenceException("Không thể khởi tạo UIHandler sau khi thử lại.");
            }
            Log.Information("UIHandler đã được khởi tạo: {IsNotNull}", uiHandler != null);
            settingsHandler = new SettingsHandler(uiHandler, this) ?? throw new NullReferenceException("Không thể khởi tạo SettingsHandler.");
            Log.Information("SettingsHandler đã được khởi tạo: {IsNotNull}", settingsHandler != null);
            logicHandler = new LogicHandler(uiHandler, settingsHandler, themeHandler) ?? throw new NullReferenceException("Không thể khởi tạo LogicHandler.");
            Log.Information("LogicHandler đã được khởi tạo: {IsNotNull}", logicHandler != null);

            uiHandler.SetLogicHandler(logicHandler);

            uiHandler.SetupUI();
            settingsHandler.LoadSettings();
            ApplyThemeSafely();

            RegisterEventHandlers();
            Log.Information("Form1 đã khởi tạo thành công.");
        }

        private void RegisterEventHandlers()
        {
            uiHandler.StartButton.Click -= StartButton_Click;
            uiHandler.StopButton.Click -= StopButton_Click;
            uiHandler.ResetButton.Click -= ResetButton_Click;
            uiHandler.UpdateButton.Click -= UpdateButton_Click;
            uiHandler.ToggleUpperButton.Click -= ToggleUpperButton_Click;
            uiHandler.ToggleLowerButton.Click -= ToggleLowerButton_Click;

            uiHandler.StartButton.Click += StartButton_Click;
            uiHandler.StopButton.Click += StopButton_Click;
            uiHandler.ResetButton.Click += ResetButton_Click;
            uiHandler.UpdateButton.Click += UpdateButton_Click;
            uiHandler.ToggleUpperButton.Click += ToggleUpperButton_Click;
            uiHandler.ToggleLowerButton.Click += ToggleLowerButton_Click;
            Log.Debug("Đã đăng ký sự kiện cho các nút thành công.");
        }

        private UIHandler CreateUIHandler()
        {
            try
            {
                Log.Information("Đang khởi tạo UIHandler với form: {Form}, themeHandler: {ThemeHandler}", this, themeHandler);
                UIHandler handler = new UIHandler(this, themeHandler, null);
                Log.Information("UIHandler đã khởi tạo thành công: {IsNotNull}", handler != null);
                return handler;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi khởi tạo UIHandler");
                return null;
            }
        }

        private void ApplyThemeSafely()
        {
            try
            {
                Log.Information("Đang áp dụng theme cho toàn bộ form và UI.");
                themeHandler.ApplyTheme(this);
                themeHandler.UpdateAllControlsTheme(this);
                Log.Information("Đã áp dụng theme cho form và UI thành công.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi áp dụng theme");
                throw new NullReferenceException($"Lỗi khi áp dụng theme: {ex.Message}", ex);
            }
        }
        public void UpdateGlobalTheme(bool isLightTheme)
        {
            themeHandler.IsLightTheme = isLightTheme;
            ApplyThemeSafely();
            Log.Information("Theme toàn cục đã được cập nhật: {Theme}", isLightTheme ? "Light" : "Dark");
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            Log.Information("Starting monitoring...");
            await logicHandler.StartMonitoring();
            Log.Information("Monitoring started successfully.");
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Log.Information("Stopping monitoring...");
            logicHandler.StopMonitoring();
            Log.Information("Monitoring stopped successfully.");
        }

        private async void ResetButton_Click(object sender, EventArgs e)
        {
            Log.Information("Resetting monitoring...");
            await logicHandler.ResetMonitoring();
            Log.Information("Monitoring reset successfully.");
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            Log.Debug("Bắt đầu cập nhật limits - Kiểm tra giá trị ngưỡng và trạng thái nút: Upper={Upper}, Lower={Lower}, " +
                "ToggleUpper={ToggleUpperState}, ToggleLower={ToggleLowerState}",
                uiHandler.UpperLimitBox.Text, uiHandler.LowerLimitBox.Text,
                uiHandler.ToggleUpperButton.Text, uiHandler.ToggleLowerButton.Text);

            string upperText = uiHandler.UpperLimitBox.Text.Replace(",", ".");
            string lowerText = uiHandler.LowerLimitBox.Text.Replace(",", ".");
            float upper, lower;

            if (float.TryParse(upperText, NumberStyles.Any, CultureInfo.InvariantCulture, out upper) &&
                float.TryParse(lowerText, NumberStyles.Any, CultureInfo.InvariantCulture, out lower))
            {
                if (uiHandler.ToggleUpperButton.Text == "Bật" && uiHandler.ToggleLowerButton.Text == "Bật")
                {
                    float currentUpper = logicHandler.GetUpdatedUpperLimit();
                    float currentLower = logicHandler.GetUpdatedLowerLimit();

                    if (upper <= lower)
                    {
                        Log.Warning("Ngưỡng không hợp lệ (Upper ≤ Lower) khi cả hai nút bật, hiển thị cảnh báo và không cập nhật.");
                        new AlertForm("Mực nước ngưỡng cao phải cao hơn ngưỡng thấp khi cả hai nút bật!", "Lỗi", themeHandler).ShowDialog(this);
                        return;
                    }
                    logicHandler.UpdateLimits(upper, lower);
                    UpdateLimitButtonsState();
                    Log.Information("Limits updated successfully with Upper={Upper}, Lower={Lower}", upper, lower);
                }
                else
                {
                    logicHandler.UpdateLimits(upper, lower);
                    UpdateLimitButtonsState();
                    Log.Information("Limits updated successfully with Upper={Upper}, Lower={Lower} (không kiểm tra logic ngưỡng)", upper, lower);
                }
            }
            else
            {
                Log.Warning("Không thể parse giá trị ngưỡng, hiển thị cảnh báo.");
                new AlertForm("Giá trị ngưỡng không hợp lệ! Vui lòng nhập số hợp lệ.", "Lỗi", themeHandler).ShowDialog(this);
            }
        }

        private void ToggleUpperButton_Click(object sender, EventArgs e)
        {
            Log.Debug("Bắt đầu toggle upper limit - Giá trị hiện tại: UpdatedUpper={UpdatedUpper}, UpdatedLower={UpdatedLower}, " +
                "ToggleUpper={ToggleUpperState}, ToggleLower={ToggleLowerState}",
                logicHandler.GetUpdatedUpperLimit(), logicHandler.GetUpdatedLowerLimit(),
                uiHandler.ToggleUpperButton.Text, uiHandler.ToggleLowerButton.Text);
            logicHandler.SetTogglingLimits(true);
            float upper = logicHandler.GetUpdatedUpperLimit();
            float lower = logicHandler.GetUpdatedLowerLimit();

            if (uiHandler.ToggleUpperButton.Text == "Tắt" && uiHandler.ToggleLowerButton.Text == "Bật" && upper <= lower)
            {
                Log.Warning("Ngưỡng không hợp lệ (UpdatedUpper ≤ UpdatedLower) khi cố gắng bật ToggleUpper, hiển thị cảnh báo và không toggle.");
                new AlertForm("Mực nước ngưỡng cao phải cao hơn ngưỡng thấp khi bật kiểm tra ngưỡng thấp!", "Lỗi", themeHandler).ShowDialog(this);
            }
            else if (uiHandler.ToggleUpperButton.Text == "Tắt")
            {
                uiHandler.ToggleUpperButton.Enabled = true;
                uiHandler.ToggleLowerButton.Enabled = true;
                uiHandler.ToggleUpper();
                Log.Information("Upper limit toggled successfully.");
            }
            else
            {
                uiHandler.ToggleUpperButton.Enabled = true;
                uiHandler.ToggleLowerButton.Enabled = true;
                uiHandler.ToggleUpper();
                Log.Information("Upper limit toggled successfully.");
            }

            Log.Debug("Hoàn thành toggle upper limit - Giá trị mới: UpdatedUpper={UpdatedUpper}, UpdatedLower={UpdatedLower}, " +
                "ToggleUpper={ToggleUpperState}, ToggleLower={ToggleLowerState}",
                logicHandler.GetUpdatedUpperLimit(), logicHandler.GetUpdatedLowerLimit(),
                uiHandler.ToggleUpperButton.Text, uiHandler.ToggleLowerButton.Text);
            logicHandler.SetTogglingLimits(false);
        }

        private void ToggleLowerButton_Click(object sender, EventArgs e)
        {
            Log.Debug("Bắt đầu toggle lower limit - Giá trị hiện tại: UpdatedUpper={UpdatedUpper}, UpdatedLower={UpdatedLower}, " +
                "ToggleUpper={ToggleUpperState}, ToggleLower={ToggleLowerState}",
                logicHandler.GetUpdatedUpperLimit(), logicHandler.GetUpdatedLowerLimit(),
                uiHandler.ToggleUpperButton.Text, uiHandler.ToggleLowerButton.Text);
            logicHandler.SetTogglingLimits(true);
            float upper = logicHandler.GetUpdatedUpperLimit();
            float lower = logicHandler.GetUpdatedLowerLimit();

            if (uiHandler.ToggleLowerButton.Text == "Tắt" && uiHandler.ToggleUpperButton.Text == "Bật" && upper <= lower)
            {
                Log.Warning("Ngưỡng không hợp lệ (UpdatedUpper ≤ UpdatedLower) khi cố gắng bật ToggleLower, hiển thị cảnh báo và không toggle.");
                new AlertForm("Mực nước ngưỡng thấp phải thấp hơn ngưỡng cao khi bật kiểm tra ngưỡng cao!", "Lỗi", themeHandler).ShowDialog(this);
            }
            else if (uiHandler.ToggleLowerButton.Text == "Tắt")
            {
                uiHandler.ToggleUpperButton.Enabled = true;
                uiHandler.ToggleLowerButton.Enabled = true;
                uiHandler.ToggleLower();
                Log.Information("Lower limit toggled successfully.");
            }
            else
            {
                uiHandler.ToggleUpperButton.Enabled = true;
                uiHandler.ToggleLowerButton.Enabled = true;
                uiHandler.ToggleLower();
                Log.Information("Lower limit toggled successfully.");
            }

            Log.Debug("Hoàn thành toggle lower limit - Giá trị mới: UpdatedUpper={UpdatedUpper}, UpdatedLower={UpdatedLower}, " +
                "ToggleUpper={ToggleUpperState}, ToggleLower={ToggleLowerState}",
                logicHandler.GetUpdatedUpperLimit(), logicHandler.GetUpdatedLowerLimit(),
                uiHandler.ToggleUpperButton.Text, uiHandler.ToggleLowerButton.Text);
            logicHandler.SetTogglingLimits(false);
        }

        public void SettingsButton_Click(object sender, EventArgs e)
        {
            Log.Debug("SettingsButton_Click được gọi.");
            uiHandler.OpenSettingsForm();
        }

        public void TopMostButton_Click(object sender, EventArgs e)
        {
            Log.Debug("TopMostButton_Click được gọi.");
            uiHandler.TopMostButton_Click(sender, e);
        }

        public void TutorialButton_Click(object sender, EventArgs e)
        {
            Log.Debug("TutorialButton_Click được gọi.");
            uiHandler.TutorialButton_Click(sender, e);
        }

        public void UpdateLimitButtonsState()
        {
            Log.Debug("Cập nhật trạng thái nút dựa trên giá trị ngưỡng - Giá trị hiện tại: UpdatedUpper={UpdatedUpper}, UpdatedLower={UpdatedLower}",
                logicHandler.GetUpdatedUpperLimit(), logicHandler.GetUpdatedLowerLimit());
            if (logicHandler.GetUpdatedUpperLimit() > 0 && logicHandler.GetUpdatedLowerLimit() > 0)
            {
                uiHandler.ToggleUpperButton.Enabled = true;
                uiHandler.ToggleLowerButton.Enabled = true;
                Log.Information("Nút ToggleUpper và ToggleLower được bật (sử dụng ngưỡng đã cập nhật).");
            }
            else
            {
                uiHandler.ToggleUpperButton.Enabled = false;
                uiHandler.ToggleLowerButton.Enabled = false;
                Log.Warning("Ngưỡng chưa được cập nhật, tắt nút ToggleUpper và ToggleLower.");
            }
        }

        public UIHandler UI => uiHandler ?? throw new NullReferenceException("UIHandler chưa được khởi tạo.");
        public ThemeHandler ThemeHandler => themeHandler ?? throw new NullReferenceException("ThemeHandler chưa được khởi tạo.");
        public SettingsHandler SettingsHandler => settingsHandler ?? throw new NullReferenceException("SettingsHandler chưa được khởi tạo.");
    }
}