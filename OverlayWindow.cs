using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WF = System.Windows.Forms;

namespace BreakReminder
{
    class OverlayWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint flags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_SHOWWINDOW = 0x0040;

        readonly System.Drawing.Rectangle bounds;
        readonly TextBlock timeText;

        public OverlayWindow(WF.Screen screen, Action onSkip)
        {
            bounds = screen.Bounds;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(230, 10, 10, 10));
            WindowStartupLocation = WindowStartupLocation.Manual;

            var title = new TextBlock
            {
                Text = "该休息一下了",
                FontSize = 40,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timeText = new TextBlock
            {
                FontSize = 110,
                FontWeight = FontWeights.Light,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 30)
            };
            var skip = new Button
            {
                Content = "跳过 (Esc)",
                FontSize = 18,
                Padding = new Thickness(28, 8, 28, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                Cursor = Cursors.Hand
            };
            skip.Click += (s, e) => onSkip();

            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            panel.Children.Add(title);
            panel.Children.Add(timeText);
            panel.Children.Add(skip);
            Content = panel;

            KeyDown += (s, e) => { if (e.Key == Key.Escape) onSkip(); };

            SourceInitialized += (s, e) => Place();
            ContentRendered += (s, e) => { Place(); Activate(); Focus(); };
        }

        // 直接用物理像素定位，避开多屏不同缩放比例的换算问题
        void Place()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hwnd, HWND_TOPMOST, bounds.Left, bounds.Top,
                bounds.Width, bounds.Height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        public void SetTime(string text)
        {
            timeText.Text = text;
        }
    }
}
