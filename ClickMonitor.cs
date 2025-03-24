using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace ClickLimiter {
    public class AppSettings {
        public int ClickLimit { get; set; } = 5;
        public int UnlockTime { get; set; } = 5000;
        public bool BlockClicks { get; set; } = true;
        public bool ShowTrayIcon { get; set; } = true; // Default to show tray icon
    }


    public class ClickMonitor : ApplicationContext {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        private int _clickCount = 0;
        private int _clickLimit;
        private int _unlockTime;
        private bool _blockClicks;
        private bool _showTrayIcon;   // Add field for show tray icon setting
        private NotifyIcon _notifyIcon;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc _proc;
        private LimitReachedForm _limitForm;
        private bool _limitReachedDialogShown = false;
        private string _configFilePath = "config.json";
        private AppSettings _settings;

        public ClickMonitor() {
            LoadSettings();

            // Initialize tray icon based on setting
            _notifyIcon = new NotifyIcon {
                Icon = SystemIcons.Application,
                Text = "ClickGuardian",
                Visible = _showTrayIcon // Initially set visibility based on setting
            };

            if (_showTrayIcon)  //Only create the menu if we're showing the tray icon
            {
                // Create context menu for tray icon
                var menu = new ContextMenuStrip();
                menu.Items.Add("Settings", null, ShowSettings);
                menu.Items.Add("Exit", null, Exit);
                _notifyIcon.ContextMenuStrip = menu;
            }


            // Set up mouse hook
            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        private void LoadSettings() {
            if (File.Exists(_configFilePath)) {
                try {
                    string json = File.ReadAllText(_configFilePath);
                    _settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    _clickLimit = _settings.ClickLimit;
                    _unlockTime = _settings.UnlockTime;
                    _blockClicks = _settings.BlockClicks;
                    _showTrayIcon = _settings.ShowTrayIcon;  // Load tray icon setting
                } catch (Exception ex) {
                    MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Use default values
                    _settings = new AppSettings();
                    _clickLimit = _settings.ClickLimit;
                    _unlockTime = _settings.UnlockTime;
                    _blockClicks = _settings.BlockClicks;
                    _showTrayIcon = _settings.ShowTrayIcon;   // Use default
                }
            } else {
                // Use default values
                _settings = new AppSettings();
                _clickLimit = _settings.ClickLimit;
                _unlockTime = _settings.UnlockTime;
                _blockClicks = _settings.BlockClicks;
                _showTrayIcon = _settings.ShowTrayIcon;   // Use default
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
                // Clean up tray icon
                if (_notifyIcon != null) {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }

                // Unhook mouse
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

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN)) {
                _clickCount++;

                if (_showTrayIcon) // Only update if tray icon is visible
                {
                    _notifyIcon.Text = $"ClickGuardian ({_clickCount}/{_clickLimit})";
                }


                if (_clickCount >= _clickLimit && !_limitReachedDialogShown) {
                    _limitReachedDialogShown = true;
                    ShowLimitReachedDialog();
                    _clickCount = 0;
                    return _blockClicks ? (IntPtr)1 : CallNextHookEx(_hookId, nCode, wParam, lParam);
                } else if (_clickCount >= _clickLimit) {
                    return _blockClicks ? (IntPtr)1 : CallNextHookEx(_hookId, nCode, wParam, lParam);
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void ShowLimitReachedDialog() {
            if (_limitForm == null || _limitForm.IsDisposed) {
                _limitForm = new LimitReachedForm(_clickLimit, _unlockTime);
                _limitForm.FormClosed += (s, args) => { _limitReachedDialogShown = false; };
                _limitForm.Show();
            } else {
                _limitForm.BringToFront();
            }
        }
        private void ShowSettings(object sender, EventArgs e) {
            using (var form = new SettingsForm(_clickLimit, _unlockTime, _blockClicks, _showTrayIcon)) {
                if (form.ShowDialog() == DialogResult.OK) {
                    _clickLimit = form.ClickLimit;
                    _unlockTime = form.UnlockTime;
                    _blockClicks = form.BlockClicks;
                    _showTrayIcon = form.ShowTrayIcon;  // Save new tray icon setting

                    _settings.ClickLimit = _clickLimit;
                    _settings.UnlockTime = _unlockTime;
                    _settings.BlockClicks = _blockClicks;
                    _settings.ShowTrayIcon = _showTrayIcon;  // Save tray icon setting
                    SaveSettings();

                    // Update tray icon visibility immediately
                    _notifyIcon.Visible = _showTrayIcon;

                    if (_showTrayIcon) {
                        _notifyIcon.Text = $"ClickGuardian ({_clickCount}/{_clickLimit})";  //Update the text now that it is visible
                    }


                    _clickCount = 0;
                    if (_showTrayIcon) {
                        _notifyIcon.Text = $"ClickGuardian ({_clickCount}/{_clickLimit})";
                    }
                }
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