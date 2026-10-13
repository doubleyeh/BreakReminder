using System;
using System.IO;

namespace BreakReminder
{
    class AppSettings
    {
        public int WorkMinutes = 45;
        public int RestMinutes = 5;

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
                        if (!int.TryParse(kv[1].Trim(), out v) || v < 1 || v > 999) continue;
                        if (kv[0].Trim() == "WorkMinutes") s.WorkMinutes = v;
                        else if (kv[0].Trim() == "RestMinutes") s.RestMinutes = v;
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
                    "RestMinutes=" + RestMinutes
                });
            }
            catch { }
        }
    }
}
