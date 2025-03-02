using System.Drawing;
using System.Windows.Forms;
using WaterLevelMonitor;
using Serilog;
using System;

namespace WaterLevelMonitor
{
    public class ThemeHandler
    {
        private readonly Form1 form;
        public bool IsLightTheme { get; set; } = true;

        public ThemeHandler(Form1 form)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form), "Form không thể là null.");
        }

        public void ApplyTheme(Form targetForm = null)
        {
            Form applyForm = targetForm ?? form ?? throw new NullReferenceException("Không có form nào được cung cấp để áp dụng theme.");
            applyForm.BackColor = IsLightTheme ? ColorTranslator.FromHtml("#FFFFFF") : ColorTranslator.FromHtml("#202020");
            try
            {
                UIHandler ui;
                try
                {
                    ui = form.UI;
                }
                catch (Exception ex)
                {
                    Form1.Log.Error(ex, "Lỗi khi truy cập form.UI trong ThemeHandler.ApplyTheme");
                    throw;
                }

                if (ui == null)
                {
                    Form1.Log.Warning("UIHandler là null trong ThemeHandler.ApplyTheme, bỏ qua áp dụng theme.");
                    return;
                }

                Log.Information("Bắt đầu áp dụng theme cho form.");
                foreach (Control c in applyForm.Controls)
                {
                    if (c is Panel panel)
                    {
                        if (panel.Height == 60 && panel.Controls.Contains(ui.StartButton))
                            panel.BackColor = IsLightTheme ? ColorTranslator.FromHtml("#F3F3F3") : ColorTranslator.FromHtml("#2D2D2D");
                        else
                            panel.BackColor = IsLightTheme ? ColorTranslator.FromHtml("#FFFFFF") : ColorTranslator.FromHtml("#202020");
                        foreach (Control pc in panel.Controls)
                        {
                            if (pc is Label lbl)
                                lbl.ForeColor = IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3");
                            else if (pc is Button btn && btn != ui.StartButton && btn != ui.StopButton && btn != ui.ResetButton && btn != ui.UpdateButton && btn != ui.ToggleUpperButton && btn != ui.ToggleLowerButton)
                            {
                                btn.BackColor = IsLightTheme ? ColorTranslator.FromHtml("#F3F3F3") : ColorTranslator.FromHtml("#404040");
                                btn.ForeColor = IsLightTheme ? Color.Black : Color.White;
                            }
                            else if (pc is TextBox txt)
                            {
                                txt.BackColor = IsLightTheme ? Color.White : ColorTranslator.FromHtml("#404040");
                                txt.ForeColor = IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3");
                            }
                        }
                    }
                    else if (c is Label lbl)
                    {
                        if (lbl != ui.LevelLabel && lbl != ui.StatusLabel && lbl != ui.LimitsLabel)
                            lbl.ForeColor = IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3");
                    }
                }
                if (applyForm == form)
                {
                    ui.LevelLabel.ForeColor = IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4");
                    ui.StatusLabel.ForeColor = IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3");
                    ui.LimitsLabel.ForeColor = IsLightTheme ? Color.Black : ColorTranslator.FromHtml("#D3D3D3");
                    UpdateButtonColor(ui.StartButton);
                    UpdateButtonColor(ui.StopButton);
                    UpdateButtonColor(ui.ResetButton);
                    UpdateButtonColor(ui.UpdateButton);
                    UpdateToggleButtonColor(ui.ToggleUpperButton);
                    UpdateToggleButtonColor(ui.ToggleLowerButton);
                    UpdateButtonColor(ui.SettingsButton);
                    UpdateButtonColor(ui.TopMostButton);
                    UpdateButtonColor(ui.TutorialButton);
                }
                Form1.Log.Information("Đã áp dụng theme cho form thành công.");
            }
            catch (Exception ex)
            {
                Form1.Log.Error(ex, "Lỗi khi áp dụng theme");
                throw new NullReferenceException($"Lỗi khi áp dụng theme: {ex.Message}", ex);
            }
        }

        public void UpdateAllControlsTheme(Control control)
        {
            if (control is Form formControl)
            {
                ApplyTheme(formControl);
                formControl.Refresh();
            }
        }

        public void UpdateButtonColor(Button button)
        {
            button.BackColor = IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4");
            button.ForeColor = Color.White;
        }

        public void UpdateToggleButtonColor(Button button)
        {
            button.BackColor = button.Text == "Bật" ? (IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4")) : (IsLightTheme ? ColorTranslator.FromHtml("#A0A0A0") : ColorTranslator.FromHtml("#666666"));
            button.ForeColor = Color.White;
        }
    }
}