using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
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

        const string ButtonTemplateXaml = @"
<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                 xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                 TargetType='Button'>
  <Border x:Name='bd' CornerRadius='24' Background='#1FFFFFFF'
          BorderBrush='#4DFFFFFF' BorderThickness='1'
          Padding='{TemplateBinding Padding}'>
    <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
  </Border>
  <ControlTemplate.Triggers>
    <Trigger Property='IsMouseOver' Value='True'>
      <Setter TargetName='bd' Property='Background' Value='#40FFFFFF'/>
      <Setter TargetName='bd' Property='BorderBrush' Value='#99FFFFFF'/>
    </Trigger>
    <Trigger Property='IsPressed' Value='True'>
      <Setter TargetName='bd' Property='Background' Value='#66FFFFFF'/>
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>";

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
            Background = new SolidColorBrush(Color.FromArgb(235, 12, 14, 20));
            WindowStartupLocation = WindowStartupLocation.Manual;

            var title = new TextBlock
            {
                Text = "该休息一下了",
                FontFamily = new FontFamily("Segoe UI Light, Microsoft YaHei UI Light"),
                FontSize = 36,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            timeText = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI Light"),
                FontSize = 128,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 12, 0, 36)
            };
            var skip = new Button
            {
                Content = "跳过休息  ·  Esc",
                FontFamily = new FontFamily("Segoe UI, Microsoft YaHei UI"),
                FontSize = 16,
                Foreground = Brushes.White,
                Padding = new Thickness(36, 11, 36, 11),
                HorizontalAlignment = HorizontalAlignment.Center,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                Template = (ControlTemplate)XamlReader.Parse(ButtonTemplateXaml)
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
