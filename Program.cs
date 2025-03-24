using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;


namespace ClickLimiter {
    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            // When started from elsewhere like by the system, move where assets and files are
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Force smooth text rendering globally
            Application.Run(new ClickMonitor());
        }
    }
}
