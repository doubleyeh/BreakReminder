using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace BreakReminder
{
    // 常驻桌面的小悬浮倒计时：可拖动，双击隐藏（托盘菜单可重新打开）
    class MiniClock : Window
    {
        readonly TextBlock text;

        public MiniClock(Action onHide)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;

            text = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            var border = new Border
            {
                Margin = new Thickness(8),
                Padding = new Thickness(16, 7, 16, 7),
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Color.FromArgb(215, 28, 28, 32)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                Effect = new DropShadowEffect { BlurRadius = 12, ShadowDepth = 0, Opacity = 0.45 },
                Child = text,
                ToolTip = "拖动移动，双击隐藏"
            };
            Content = border;

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2) onHide();
                else DragMove();
            };

            Loaded += (s, e) =>
            {
                var wa = SystemParameters.WorkArea;
                Left = wa.Right - ActualWidth - 16;
                Top = wa.Top + 16;
            };
        }

        public void SetText(string t)
        {
            text.Text = t;
        }
    }
}
