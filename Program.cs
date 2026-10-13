using System;
using System.Threading;
using System.Windows;

namespace BreakReminder
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool created;
            using (var mutex = new Mutex(true, "BreakReminder.SingleInstance", out created))
            {
                if (!created) return;

                var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                var controller = new Controller();
                controller.Start();
                app.Run();
            }
        }
    }
}
