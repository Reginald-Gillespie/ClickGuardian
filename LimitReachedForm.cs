// LimitReachedForm.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClickLimiter {
    public class LimitReachedForm : Form {
        private System.Windows.Forms.Timer _unlockTimer; // Explicit reference to Windows.Forms.Timer
        private bool _allowClicks = false;
        private int _unlockTime;

        public LimitReachedForm(int clickLimit, int unlockTime) {
            _unlockTime = unlockTime; // Store unlock time

            // Window properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30); // Dark mode
            this.TopMost = true;

            // Title
            Label title = new Label()
            {
                Text = "Upgrade Required: Monthly Click Limit Reached",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // Subtext
            Label subtext = new Label()
            {
                Text = $"You have reached the maximum number of clicks allowed ({clickLimit}).\nTo continue using your mouse, please upgrade to a plan.",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, 50),
                AutoSize = true
            };

            // Create a panel to hold subscription plans
            Panel subscriptionPanel = new Panel()
            {
                Size = new Size(460, 140),
                Location = new Point(20, 90),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            // Standard Plan
            Label standardPlan = new Label()
            {
                Text = "Standard Plan - $10.99/month",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            Label standardFeatures = new Label()
            {
                Text = "• 10,000 clicks per month\n• Adjustable button mappings",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 35),
                AutoSize = true
            };

            Button standardButton = new Button()
            {
                Text = "Upgrade to Standard",
                Size = new Size(180, 30),
                Location = new Point(10, 85),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            standardButton.Click += (s, e) =>
            {
                MessageBox.Show("Congratulations! You have subscribed to our Standard plan.\nEnjoy your premium mouse experience!", "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            // Premium Plan
            Label premiumPlan = new Label()
            {
                Text = "Premium Plan - $17.99/month",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(250, 10),
                AutoSize = true
            };

            Label premiumFeatures = new Label()
            {
                Text = "• Unlimited clicks\n• Custom button mappings\n• Priority support",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(250, 35),
                AutoSize = true
            };

            Button premiumButton = new Button()
            {
                Text = "Upgrade to Premium",
                Size = new Size(180, 30),
                Location = new Point(250, 85),
                BackColor = Color.Gold,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };

            premiumButton.Click += (s, e) =>
           {
               MessageBox.Show("Congratulations! You have subscribed to our Premium plan.\nEnjoy your premium mouse experience!", "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
               this.Close();
           };

            // "Remind Me Later" button
            Button remindLaterButton = new Button()
            {
                Text = "Remind Me Later",
                Size = new Size(200, 35),
                Location = new Point(150, 250),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            remindLaterButton.Click += (s, e) =>
            {
                // Start timer to re-enable clicks after specified time
                _unlockTimer = new System.Windows.Forms.Timer { Interval = _unlockTime }; // Use _unlockTime
                _unlockTimer.Tick += (sender, args) =>
                {
                    _unlockTimer.Stop();
                    this.Close();
                };
                _unlockTimer.Start();

                remindLaterButton.Text = $"Unlocking in {_unlockTime/1000}s..."; // Show time in seconds
                remindLaterButton.Enabled = false;
            };

            // Add a warning for the joke
            var disclaimerLabel = new Label
            {
                Text = "This is a joke application - use responsibly!",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(40, 295),
                Size = new Size(320, 30),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls
            subscriptionPanel.Controls.Add(standardPlan);
            subscriptionPanel.Controls.Add(standardFeatures);
            subscriptionPanel.Controls.Add(standardButton);
            subscriptionPanel.Controls.Add(premiumPlan);
            subscriptionPanel.Controls.Add(premiumFeatures);
            subscriptionPanel.Controls.Add(premiumButton);

            this.Controls.Add(title);
            this.Controls.Add(subtext);
            this.Controls.Add(subscriptionPanel);
            this.Controls.Add(remindLaterButton);
            this.Controls.Add(disclaimerLabel);

            this.FormClosing += (s, e) => _allowClicks = true; // Allow normal behavior on close
        }
        protected override void WndProc(ref Message m)
        {
            // Allow the form to receive mouse events but block them from going through
            // to applications underneath unless we've explicitly allowed it
            const int WM_NCHITTEST = 0x0084;
            const int HTCLIENT = 1;

            if (m.Msg == WM_NCHITTEST && !_allowClicks)
            {
                m.Result = (IntPtr)HTCLIENT;
                return;
            }

            base.WndProc(ref m);
        }
    }
}