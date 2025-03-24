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

            // Force smooth text rendering globally
            Application.Run(new ClickMonitor());
        }
    }
}
