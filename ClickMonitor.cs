using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Reflection;


namespace ClickLimiter {
    public class AppSettings {
        public int ClickLimit { get; set; } = 10;
        public int GracePeriod { get; set; } = 8;
        public int UnlockTime { get; set; } = 5000;
        public bool BlockClicks { get; set; } = true;
        public bool ShowTrayIcon { get; set; } = true;
        public bool ShowExitButton { get; set; } = true;
        public int CurrentClickCount { get; set; } = 0;
        public int LastResetYear { get; set; } = DateTime.Now.Year;
        public int LastResetMonth { get; set; } = DateTime.Now.Month;
        public bool RegisterForReboot { get; set; } = true;
        public bool RedirectToGithub { get; set; } = true;
    }

    public class ClickMonitor : ApplicationContext {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        private int _clickCount = 0;
        private int _clickLimit;
        private int _unlockTime;
        private bool _blockClicks;
        private bool _showTrayIcon;
        private bool _showExitButton;
        private int _gracePeriod;
        private bool _registerForReboot;
        private bool _redirectToGithub;
        private NotifyIcon _notifyIcon;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc _proc;
        private LimitReachedForm _limitForm;
        private bool _limitReachedDialogShown = false;
        private bool _blockingClicks = false;
        private string _configFilePath = "config.json";
        private AppSettings _settings;

        private const string StartupKeyName = "ClickGuardian";

        private void RemoveStartup() {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)) {
                if (key != null) {
                    key.DeleteValue(StartupKeyName, false);
                }
            }
        }
        private void EnsureStartup() {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)) {
                if (key != null) {
                    object existingValue = key.GetValue(StartupKeyName);

                    if (existingValue == null || existingValue.ToString() != $"\"{exePath}\"") {
                        key.SetValue(StartupKeyName, $"\"{exePath}\""); // Ensures correct quoting for paths with spaces
                    }
                }
            }
        }
        public ClickMonitor() {
            EnsureStartup();
            LoadSettings();

            _notifyIcon = new NotifyIcon {
                Icon = new Icon("assets/tray.ico"),
                Text = $"ClickGuardian | {_clickCount}/{_clickLimit} clicks",
                Visible = _showTrayIcon
            };

            if (_showTrayIcon && _showExitButton) {
                var menu = new ContextMenuStrip();
                menu.Items.Add("Exit", null, Exit);
                _notifyIcon.ContextMenuStrip = menu;
            }

            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        private void LoadSettings() {
            if (File.Exists(_configFilePath)) {
                try {
                    string json = File.ReadAllText(_configFilePath);
                    _settings = JsonConvert.DeserializeObject<AppSettings>(json);

                    // Check if the month has changed since the last reset
                    var now = DateTime.Now;
                    if (_settings.LastResetYear != now.Year || _settings.LastResetMonth != now.Month) {
                        _settings.CurrentClickCount = 0;
                        _settings.LastResetYear = now.Year;
                        _settings.LastResetMonth = now.Month;
                        SaveSettings(); // Save the reset state
                    }
                    _clickCount = _settings.CurrentClickCount;
                    _clickLimit = _settings.ClickLimit;
                    _unlockTime = _settings.UnlockTime;
                    _blockClicks = _settings.BlockClicks;
                    _showTrayIcon = _settings.ShowTrayIcon;
                    _showExitButton = _settings.ShowExitButton;
                    _gracePeriod = _settings.GracePeriod;

                    //LOAD NEW SETTINGS:
                    _registerForReboot = _settings.RegisterForReboot;
                    _redirectToGithub = _settings.RedirectToGithub;

                    if (_registerForReboot) EnsureStartup();
                    else RemoveStartup();

                } catch (Exception ex) {
                    MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _settings = new AppSettings();
                    _clickCount = _settings.CurrentClickCount;
                    _clickLimit = _settings.ClickLimit;
                    _unlockTime = _settings.UnlockTime;
                    _blockClicks = _settings.BlockClicks;
                    _showTrayIcon = _settings.ShowTrayIcon;
                    _showExitButton = _settings.ShowExitButton;
                    _gracePeriod = _settings.GracePeriod;

                    //LOAD NEW SETTINGS:
                    _registerForReboot = _settings.RegisterForReboot;
                    _redirectToGithub = _settings.RedirectToGithub;

                    if (_registerForReboot) EnsureStartup();
                    else RemoveStartup();
                }
            } else {
                _settings = new AppSettings();
                _clickCount = _settings.CurrentClickCount;
                _clickLimit = _settings.ClickLimit;
                _unlockTime = _settings.UnlockTime;
                _blockClicks = _settings.BlockClicks;
                _showTrayIcon = _settings.ShowTrayIcon;
                _showExitButton = _settings.ShowExitButton;
                _gracePeriod = _settings.GracePeriod;

                //LOAD NEW SETTINGS:
                _registerForReboot = _settings.RegisterForReboot;
                _redirectToGithub = _settings.RedirectToGithub;

                if (_registerForReboot) EnsureStartup();
                else RemoveStartup();
                SaveSettings();
            }
        }
        private void SaveSettings() {
            try {
                string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
            } catch (Exception ex) {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_notifyIcon != null) {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
                if (_hookId != IntPtr.Zero) {
                    UnhookWindowsHookEx(_hookId);
                }
            }
            base.Dispose(disposing);
        }

        private IntPtr SetHook(LowLevelMouseProc proc) {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public uint mouseData;
            uint flags;
            uint time;
            public IntPtr dwExtraInfo;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)) {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                Point mousePos = new Point(hookStruct.pt.x, hookStruct.pt.y);

                // Check if the month has changed before incrementing
                var now = DateTime.Now;
                if (now.Year != _settings.LastResetYear || now.Month != _settings.LastResetMonth) {
                    _clickCount = 0;
                    _settings.CurrentClickCount = 0;
                    _settings.LastResetYear = now.Year;
                    _settings.LastResetMonth = now.Month;
                    SaveSettings();
                }

                _clickCount++;
                _settings.CurrentClickCount = _clickCount;
                SaveSettings(); // Save the updated click count

                if (_showTrayIcon) {
                    _notifyIcon.Text = $"ClickGuardian | {_clickCount}/{_clickLimit} clicks";
                }

                if (_blockingClicks && _blockClicks) {
                    if (_limitForm != null && _limitForm.Visible && _limitForm.Bounds.Contains(mousePos)) {
                        return CallNextHookEx(_hookId, nCode, wParam, lParam);
                    }
                    return (IntPtr)1; // Block the click
                }

                if (_clickCount >= _clickLimit && !_limitReachedDialogShown) {
                    _limitReachedDialogShown = true;
                    _blockingClicks = true;
                    ShowLimitReachedDialog();
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void ShowLimitReachedDialog() {
            if (_limitForm == null || _limitForm.IsDisposed) {
                _limitForm = new LimitReachedForm(_clickLimit, _unlockTime, _redirectToGithub);
                _limitForm.OnLimitReset += () => {
                    _blockingClicks = false;
                    _clickCount = Math.Max(0, _clickLimit - _gracePeriod); // Apply GracePeriod
                    _settings.CurrentClickCount = _clickCount;
                    SaveSettings(); // Save the updated click count after reset
                };
                _limitForm.FormClosed += (s, args) => { _limitReachedDialogShown = false; };
                _limitForm.Show();
            } else {
                _limitForm.BringToFront();
            }
        }

        private void Exit(object sender, EventArgs e) {
            Application.Exit();
        }

        #region Win32 API
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}