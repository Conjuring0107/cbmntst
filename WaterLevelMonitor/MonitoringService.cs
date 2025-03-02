using Microsoft.Playwright;
using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace WaterLevelMonitor
{
    public class MonitoringService
    {
        private readonly UIHandler ui;
        private readonly CredentialManager credentials;
        private readonly ThemeHandler themeHandler;
        private readonly LogicHandler logicHandler;
        private bool running = false;
        private DateTime? lastDisconnectTime = null;
        private int disconnectThreshold = 60;
        private bool isTogglingLimits = false;

        public MonitoringService(UIHandler uiHandler, CredentialManager credentialManager, ThemeHandler themeHandler, LogicHandler logicHandler)
        {
            ui = uiHandler;
            credentials = credentialManager;
            this.themeHandler = themeHandler;
            this.logicHandler = logicHandler ?? throw new ArgumentNullException(nameof(logicHandler), "LogicHandler không thể là null.");
        }

        public void SetDisconnectThreshold(int value)
        {
            disconnectThreshold = value;
        }

        public void SetTogglingLimits(bool value)
        {
            isTogglingLimits = value;
        }

        public async Task StartMonitoring()
        {
            if (running) return;
            running = true;
            ui.Form.Invoke(new MethodInvoker(() =>
            {
                ui.StartButton.Enabled = false;
                ui.StopButton.Enabled = true;
                ui.ResetButton.Enabled = true;
                ui.StatusLabel.Text = "Trạng thái: Đang khởi động";
            }));

            await Task.Run(async () =>
            {
                IPlaywright playwright = await Playwright.CreateAsync();
                try
                {
                    string executablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ms-playwright", "chromium_headless_shell-1155", "chrome-win", "headless_shell.exe");
                    if (!File.Exists(executablePath))
                    {
                        throw new Exception($"Không tìm thấy {executablePath}. Vui lòng kiểm tra folder ms-playwright.");
                    }

                    IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true,
                        ExecutablePath = executablePath
                    });
                    try
                    {
                        IPage page = await browser.NewPageAsync();
                        bool wasConnected = true;

                        await page.GotoAsync("https://thuongsontay.thongtinquantrac.com/");
                        await page.FillAsync("input[name='username']", credentials.Username);
                        await page.FillAsync("input[name='password']", credentials.Password);
                        await page.ClickAsync("button[type='submit']");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        LogManager.LogInfo("Đăng nhập thành công, URL hiện tại: " + page.Url);

                        while (running)
                        {
                            try
                            {
                                if (page == null || page.IsClosed)
                                    throw new Exception("Page is null or closed");

                                IElementHandle levelElement = await page.QuerySelectorAsync("span.widget_calculate_value");
                                string levelText = levelElement != null ? await levelElement.InnerTextAsync() : null;
                                if (levelText != null)
                                {
                                    levelText = levelText.Replace(',', '.');
                                    float level = float.Parse(levelText, CultureInfo.InvariantCulture);
                                    ui.Form.Invoke(new MethodInvoker(() => ui.LevelLabel.Text = $"Mực nước: {level.ToString("F3", CultureInfo.InvariantCulture)} m"));

                                    if (!wasConnected)
                                    {
                                        LogManager.LogNetworkStatus(null, DateTime.Now);
                                        lastDisconnectTime = null;
                                        wasConnected = true;
                                        ui.Form.Invoke(new MethodInvoker(() => ui.ResetButton.Enabled = false));
                                    }

                                    float upperLimit = logicHandler.GetUpdatedUpperLimit();
                                    float lowerLimit = logicHandler.GetUpdatedLowerLimit();

                                    if (!isTogglingLimits)
                                    {
                                        if (ui.ToggleUpperButton.Text == "Bật" && level >= upperLimit)
                                        {
                                            ui.Form.Invoke(new MethodInvoker(() =>
                                            {
                                                ui.StatusLabel.Text = "Cảnh báo: Mực nước tăng quá cao!";
                                                using (SoundPlayer player = new SoundPlayer(Path.Combine(Application.StartupPath, "config", "high_alert.wav")))
                                                {
                                                    player.PlayLooping();
                                                    new AlertForm($"Mực nước tăng quá cao: {level.ToString("F3", CultureInfo.InvariantCulture)} m!", "Cảnh báo", themeHandler).ShowDialog(ui.Form);
                                                    player.Stop();
                                                }
                                                ui.StatusLabel.Text = "Trạng thái: Đang hoạt động bình thường";
                                            }));
                                        }
                                        else if (ui.ToggleLowerButton.Text == "Bật" && level <= lowerLimit)
                                        {
                                            ui.Form.Invoke(new MethodInvoker(() =>
                                            {
                                                ui.StatusLabel.Text = "Cảnh báo: Mực nước giảm quá thấp!";
                                                using (SoundPlayer player = new SoundPlayer(Path.Combine(Application.StartupPath, "config", "low_alert.wav")))
                                                {
                                                    player.PlayLooping();
                                                    new AlertForm($"Mực nước giảm quá thấp: {level.ToString("F3", CultureInfo.InvariantCulture)} m!", "Cảnh báo", themeHandler).ShowDialog(ui.Form);
                                                    player.Stop();
                                                }
                                                ui.StatusLabel.Text = "Trạng thái: Đang hoạt động bình thường";
                                            }));
                                        }
                                        else
                                        {
                                            ui.Form.Invoke(new MethodInvoker(() => ui.StatusLabel.Text = "Trạng thái: Đang hoạt động bình thường"));
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("Không tìm thấy dữ liệu mực nước");
                                }
                            }
                            catch (Exception ex)
                            {
                                ui.Form.Invoke(new MethodInvoker(() => ui.LevelLabel.Text = "Mực nước: Không khả dụng"));
                                LogManager.LogError($"Lỗi khi lấy dữ liệu: {ex.Message}");
                                if (wasConnected)
                                {
                                    lastDisconnectTime = DateTime.Now;
                                    LogManager.LogNetworkStatus(lastDisconnectTime, null);
                                    wasConnected = false;
                                }

                                if (lastDisconnectTime.HasValue && (DateTime.Now - lastDisconnectTime.Value).TotalSeconds > disconnectThreshold)
                                {
                                    ui.Form.Invoke(new MethodInvoker(() =>
                                    {
                                        ui.StatusLabel.Text = "Trạng thái: Mất kết nối quá lâu, ấn reset";
                                        ui.ResetButton.Enabled = true;
                                    }));
                                }
                                else
                                {
                                    try
                                    {
                                        if (browser != null)
                                            await browser.CloseAsync();
                                        browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                                        {
                                            Headless = true,
                                            ExecutablePath = executablePath
                                        });
                                        page = await browser.NewPageAsync();
                                        await page.GotoAsync("https://thuongsontay.thongtinquantrac.com/");
                                        await page.FillAsync("input[name='username']", credentials.Username);
                                        await page.FillAsync("input[name='password']", credentials.Password);
                                        await page.ClickAsync("button[type='submit']");
                                        ui.Form.Invoke(new MethodInvoker(() => ui.StatusLabel.Text = "Trạng thái: Đang kết nối lại"));
                                        wasConnected = true;
                                    }
                                    catch (Exception retryEx)
                                    {
                                        LogManager.LogError($"Lỗi khi thử kết nối lại: {retryEx.Message}");
                                        ui.Form.Invoke(new MethodInvoker(() => ui.StatusLabel.Text = "Trạng thái: Mất kết nối mạng - Đang thử lại"));
                                    }
                                }
                            }
                            await Task.Delay(5000);
                        }
                        if (browser != null)
                            await browser.CloseAsync();
                    }
                    finally
                    {
                        if (browser != null)
                            await browser.DisposeAsync();
                    }
                }
                finally
                {
                    playwright.Dispose();
                }
            });
        }

        public void StopMonitoring()
        {
            running = false;
            ui.StatusLabel.Text = "Trạng thái: Đang chờ khởi động, ấn start";
            ui.StartButton.Enabled = true;
            ui.StopButton.Enabled = false;
            ui.ResetButton.Enabled = false;
        }

        public async Task ResetMonitoring()
        {
            StopMonitoring();
            ui.Form.Invoke(new MethodInvoker(() =>
            {
                ui.StatusLabel.Text = "Trạng thái: Đang reset";
                ui.LevelLabel.Text = "Mực nước: Đang reset";
                ui.ResetButton.Enabled = false;
                lastDisconnectTime = null;
            }));
            await StartMonitoring();
        }
    }
}