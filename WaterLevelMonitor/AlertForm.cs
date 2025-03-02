using System;
using System.Drawing;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class AlertForm : Form
    {
        private readonly ThemeHandler themeHandler;

        public AlertForm(string message, string caption, ThemeHandler theme)
        {
            themeHandler = theme;
            InitializeComponents(message, caption);
        }

        private void InitializeComponents(string message, string caption)
        {
            Text = caption;
            Size = new Size(300, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = themeHandler.IsLightTheme ? Color.White : Color.FromArgb(32, 32, 32);

            Label lblMessage = new Label
            {
                Text = message,
                Size = new Size(260, 35),
                Location = new Point(20, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = themeHandler.IsLightTheme ? Color.Black : Color.White
            };

            Button btnOk = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                Location = new Point((Width - 75) / 2, 65),
                BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                ForeColor = Color.White
            };
            btnOk.Click += (s, e) => Close();

            Controls.Add(lblMessage);
            Controls.Add(btnOk);
        }
    }
}