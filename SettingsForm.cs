// SettingsForm.cs
using System;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace ClickLimiter {
    public class SettingsForm : Form {
        private NumericUpDown _clickLimitUpDown;
        private NumericUpDown _unlockTimeUpDown;
        private CheckBox _blockClicksCheckBox;
        private CheckBox _showTrayIconCheckBox;  // NEW: Checkbox for tray icon
        private Label _clickLimitLabel;
        private Label _unlockTimeLabel;
        private Label _blockClicksLabel;
        private Label _showTrayIconLabel;  // NEW: Label for tray icon
        private Button _saveButton;
        private Button _cancelButton;

        public int ClickLimit { get; private set; }
        public int UnlockTime { get; private set; }
        public bool BlockClicks { get; private set; }
        public bool ShowTrayIcon { get; private set; } // NEW: Property for tray icon visibility

        public SettingsForm(int clickLimit, int unlockTime = 5000, bool blockClicks = true, bool showTrayIcon = true) {
            ClickLimit = clickLimit;
            UnlockTime = unlockTime;
            BlockClicks = blockClicks;
            ShowTrayIcon = showTrayIcon; // Initialize the tray icon visibility

            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents() {
            this.Text = "Settings";
            this.Size = new System.Drawing.Size(300, 260); // Increased height
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            _clickLimitLabel = new Label() { Text = "Click Limit:", Location = new System.Drawing.Point(10, 10), AutoSize = true };
            _clickLimitUpDown = new NumericUpDown() { Location = new System.Drawing.Point(150, 10), Value = ClickLimit, Minimum = 1, Maximum = 10000 };

            _unlockTimeLabel = new Label() { Text = "Unlock Time (ms):", Location = new System.Drawing.Point(10, 50), AutoSize = true };
            _unlockTimeUpDown = new NumericUpDown() { Location = new System.Drawing.Point(150, 50), Value = UnlockTime, Minimum = 100, Maximum = 60000, Increment = 100 };

            _blockClicksLabel = new Label() { Text = "Block Clicks:", Location = new System.Drawing.Point(10, 90), AutoSize = true };
            _blockClicksCheckBox = new CheckBox() { Location = new System.Drawing.Point(150, 90), Checked = BlockClicks };

            _showTrayIconLabel = new Label() { Text = "Show Tray Icon:", Location = new System.Drawing.Point(10, 130), AutoSize = true }; // NEW: Label
            _showTrayIconCheckBox = new CheckBox() { Location = new System.Drawing.Point(150, 130), Checked = ShowTrayIcon }; // NEW: Checkbox

            _saveButton = new Button() { Text = "Save", Location = new System.Drawing.Point(50, 170), DialogResult = DialogResult.OK }; // Shift down
            _cancelButton = new Button() { Text = "Cancel", Location = new System.Drawing.Point(150, 170), DialogResult = DialogResult.Cancel }; // Shift down

            _saveButton.Click += SaveButton_Click;

            this.Controls.Add(_clickLimitLabel);
            this.Controls.Add(_clickLimitUpDown);
            this.Controls.Add(_unlockTimeLabel);
            this.Controls.Add(_unlockTimeUpDown);
            this.Controls.Add(_blockClicksLabel);
            this.Controls.Add(_blockClicksCheckBox);
            this.Controls.Add(_showTrayIconLabel);  // Add new controls
            this.Controls.Add(_showTrayIconCheckBox); // Add new controls
            this.Controls.Add(_saveButton);
            this.Controls.Add(_cancelButton);
        }

        private void LoadSettings() {
            _clickLimitUpDown.Value = ClickLimit;
            _unlockTimeUpDown.Value = UnlockTime;
            _blockClicksCheckBox.Checked = BlockClicks;
            _showTrayIconCheckBox.Checked = ShowTrayIcon; // Load tray icon setting
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            ClickLimit = (int)_clickLimitUpDown.Value;
            UnlockTime = (int)_unlockTimeUpDown.Value;
            BlockClicks = _blockClicksCheckBox.Checked;
            ShowTrayIcon = _showTrayIconCheckBox.Checked; // Save tray icon setting
            this.Close();
        }
    }
}