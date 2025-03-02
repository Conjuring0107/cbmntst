using System;
using System.Drawing;
using System.Windows.Forms;

namespace WaterLevelMonitor
{
    public class TutorialForm : Form
    {
        private readonly ThemeHandler themeHandler;

        public TutorialForm(ThemeHandler theme)
        {
            themeHandler = theme ?? throw new ArgumentNullException(nameof(theme), "ThemeHandler không thể là null.");
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Text = "Hướng dẫn sử dụng";
            Size = new Size(400, 350);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = themeHandler.IsLightTheme ? Color.White : Color.FromArgb(32, 32, 32);

            Label lblTutorial = new Label
            {
                Text = "1. Nhấn 'Start' để bắt đầu theo dõi mực nước và chú ý dòng trạng thái.\n" +
                       "2. Nhấn 'Stop' để dừng theo dõi.\n" +
                       "3. Nhấn 'Reset' khi mất kết nối quá lâu.\n" +
                       "4. Đặt ngưỡng cao/thấp trong ô 'Ngưỡng cao' và 'Ngưỡng thấp', sau đó nhấn 'Bật' để kích hoạt cảnh báo.\n" +
                       "5. Nhấn 'Cập nhật' để lưu ngưỡng.\n" +
                       "6. Nhấn 'Settings' để thay đổi theme và thời gian chờ (là thời gian giới hạn khi disconnect tự reset).\n" +
                       "7. Nhấn 'Pin' để ghim cửa sổ lên trên.",
                Size = new Size(360, 220),
                Location = new Point(20, 20),
                TextAlign = ContentAlignment.TopLeft,
                ForeColor = themeHandler.IsLightTheme ? Color.Black : Color.White
            };

            Button btnOk = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                Location = new Point((Width - 75) / 2, Height - 80),
                BackColor = themeHandler.IsLightTheme ? ColorTranslator.FromHtml("#0078D4") : ColorTranslator.FromHtml("#1B73C4"),
                ForeColor = Color.White
            };
            btnOk.Click += (s, e) =>
            {
                Form1.Log.Debug("Tutorial form đóng bởi nút OK.");
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(lblTutorial);
            Controls.Add(btnOk);

            FormClosed += TutorialForm_FormClosed;
        }

        private void TutorialForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Form1.Log.Debug("Tutorial form đã đóng hoàn toàn.");
            Dispose();
        }
    }
}