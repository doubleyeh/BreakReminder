using System.Windows;
using System.Windows.Controls;

namespace BreakReminder
{
    class SettingsWindow : Window
    {
        readonly AppSettings cfg;
        readonly TextBox workBox;
        readonly TextBox restBox;

        public SettingsWindow(AppSettings cfg)
        {
            this.cfg = cfg;

            Title = "设置";
            Width = 280;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;

            workBox = new TextBox { Text = cfg.WorkMinutes.ToString(), Margin = new Thickness(0, 4, 0, 12) };
            restBox = new TextBox { Text = cfg.RestMinutes.ToString(), Margin = new Thickness(0, 4, 0, 16) };
            var ok = new Button { Content = "确定", Padding = new Thickness(0, 6, 0, 6), IsDefault = true };
            ok.Click += OnOk;

            var panel = new StackPanel { Margin = new Thickness(16) };
            panel.Children.Add(new TextBlock { Text = "工作时长（分钟）" });
            panel.Children.Add(workBox);
            panel.Children.Add(new TextBlock { Text = "休息时长（分钟）" });
            panel.Children.Add(restBox);
            panel.Children.Add(ok);
            Content = panel;
        }

        void OnOk(object sender, RoutedEventArgs e)
        {
            int w, r;
            if (!int.TryParse(workBox.Text, out w) || w < 1 || w > 999 ||
                !int.TryParse(restBox.Text, out r) || r < 1 || r > 999)
            {
                MessageBox.Show(this, "请输入 1~999 的整数");
                return;
            }
            cfg.WorkMinutes = w;
            cfg.RestMinutes = r;
            DialogResult = true;
        }
    }
}
