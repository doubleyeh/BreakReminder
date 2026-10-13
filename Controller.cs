using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using WD = System.Drawing;
using WF = System.Windows.Forms;

namespace BreakReminder
{
    class Controller
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunName = "BreakReminder";

        AppSettings cfg;
        WF.NotifyIcon tray;
        WF.ToolStripMenuItem pauseItem;
        WF.ToolStripMenuItem autoItem;
        WF.ToolStripMenuItem miniItem;
        MiniClock mini;
        DispatcherTimer timer;
        readonly List<OverlayWindow> overlays = new List<OverlayWindow>();

        int remaining;
        bool resting;
        bool paused;

        public void Start()
        {
            cfg = AppSettings.Load();
            remaining = cfg.WorkMinutes * 60;

            var menu = new WF.ContextMenuStrip();
            menu.Items.Add("立即休息", null, (s, e) => StartRest());
            pauseItem = new WF.ToolStripMenuItem("暂停", null, (s, e) => TogglePause());
            menu.Items.Add(pauseItem);
            menu.Items.Add("重置计时", null, (s, e) => ResetWork());
            miniItem = new WF.ToolStripMenuItem("悬浮时间", null, (s, e) => SetMini(!cfg.ShowMini));
            miniItem.Checked = cfg.ShowMini;
            menu.Items.Add(miniItem);
            menu.Items.Add("设置...", null, (s, e) => ShowSettings());
            autoItem = new WF.ToolStripMenuItem("开机自启", null, (s, e) => ToggleAutoStart());
            autoItem.Checked = IsAutoStart();
            menu.Items.Add(autoItem);
            menu.Items.Add(new WF.ToolStripSeparator());
            menu.Items.Add("退出", null, (s, e) => Exit());

            tray = new WF.NotifyIcon
            {
                Icon = LoadIcon(),
                ContextMenuStrip = menu,
                Visible = true,
                Text = "休息提醒"
            };

            mini = new MiniClock(() => SetMini(false));
            mini.Show();

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => Tick();
            timer.Start();

            SystemEvents.SessionSwitch += OnSessionSwitch;
            UpdateTray();
        }

        static WD.Icon LoadIcon()
        {
            try
            {
                using (var st = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("app.ico"))
                    if (st != null) return new WD.Icon(st);
            }
            catch { }
            return WD.SystemIcons.Application;
        }

        void Tick()
        {
            if (paused) return;

            remaining--;

            if (resting)
            {
                if (remaining <= 0) { EndRest(); return; }
                foreach (var o in overlays) o.SetTime(Fmt(remaining));
            }
            else if (remaining <= 0)
            {
                StartRest();
                return;
            }
            UpdateTray();
        }

        void StartRest()
        {
            if (resting) return;
            paused = false;
            pauseItem.Text = "暂停";
            resting = true;
            remaining = cfg.RestMinutes * 60;

            foreach (var screen in WF.Screen.AllScreens)
            {
                var w = new OverlayWindow(screen, EndRest);
                w.SetTime(Fmt(remaining));
                overlays.Add(w);
                w.Show();
            }
            UpdateTray();
        }

        void EndRest()
        {
            CloseOverlays();
            resting = false;
            remaining = cfg.WorkMinutes * 60;
            UpdateTray();
        }

        void CloseOverlays()
        {
            foreach (var o in overlays) o.Close();
            overlays.Clear();
        }

        void ResetWork()
        {
            CloseOverlays();
            resting = false;
            remaining = cfg.WorkMinutes * 60;
            UpdateTray();
        }

        void TogglePause()
        {
            if (resting) return;
            paused = !paused;
            pauseItem.Text = paused ? "继续" : "暂停";
            UpdateTray();
        }

        void ShowSettings()
        {
            var win = new SettingsWindow(cfg);
            if (win.ShowDialog() == true)
            {
                cfg.Save();
                if (!resting) remaining = cfg.WorkMinutes * 60;
                UpdateTray();
            }
        }

        // 锁屏/解锁后重新计时，避免解锁后立刻弹遮罩
        void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock ||
                e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(ResetWork));
            }
        }

        void UpdateTray()
        {
            string t = resting ? "休息中 " + Fmt(remaining)
                     : paused ? "已暂停"
                     : "距离休息 " + Fmt(remaining);
            tray.Text = t.Length > 63 ? t.Substring(0, 63) : t;
            UpdateMini();
        }

        void UpdateMini()
        {
            if (mini == null) return;
            bool visible = cfg.ShowMini && !resting;
            mini.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
            if (visible) mini.SetText(paused ? "已暂停" : Fmt(remaining));
        }

        void SetMini(bool on)
        {
            cfg.ShowMini = on;
            miniItem.Checked = on;
            cfg.Save();
            UpdateMini();
        }

        static string Fmt(int sec)
        {
            if (sec < 0) sec = 0;
            return (sec / 60).ToString("00") + ":" + (sec % 60).ToString("00");
        }

        bool IsAutoStart()
        {
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey))
                return k != null && k.GetValue(RunName) != null;
        }

        void ToggleAutoStart()
        {
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (k == null) return;
                if (IsAutoStart())
                    k.DeleteValue(RunName, false);
                else
                    k.SetValue(RunName, "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
            }
            autoItem.Checked = IsAutoStart();
        }

        void Exit()
        {
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            timer.Stop();
            CloseOverlays();
            mini.Close();
            tray.Visible = false;
            tray.Dispose();
            Application.Current.Shutdown();
        }
    }
}
