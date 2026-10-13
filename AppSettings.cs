using System;
using System.IO;

namespace BreakReminder
{
    class AppSettings
    {
        public int WorkMinutes = 45;
        public int RestMinutes = 5;
        public bool ShowMini = true;

        static string FilePath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BreakReminder");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "settings.txt");
            }
        }

        public static AppSettings Load()
        {
            var s = new AppSettings();
            try
            {
                if (File.Exists(FilePath))
                {
                    foreach (var line in File.ReadAllLines(FilePath))
                    {
                        var kv = line.Split(new[] { '=' }, 2);
                        if (kv.Length != 2) continue;
                        int v;
                        if (!int.TryParse(kv[1].Trim(), out v)) continue;
                        var key = kv[0].Trim();
                        if (key == "WorkMinutes" && v >= 1 && v <= 999) s.WorkMinutes = v;
                        else if (key == "RestMinutes" && v >= 1 && v <= 999) s.RestMinutes = v;
                        else if (key == "ShowMini") s.ShowMini = v != 0;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                File.WriteAllLines(FilePath, new[]
                {
                    "WorkMinutes=" + WorkMinutes,
                    "RestMinutes=" + RestMinutes,
                    "ShowMini=" + (ShowMini ? 1 : 0)
                });
            }
            catch { }
        }
    }
}
