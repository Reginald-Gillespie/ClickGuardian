// LimitReachedForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClickLimiter {
    public class LimitReachedForm : Form {
        private System.Windows.Forms.Timer _unlockTimer;
        private bool _allowClicks = false;
        private int _unlockTime;
        private Image _arrowImage;

        // Centralized plan data
        private List<PlanData> _plans = new List<PlanData>() {
            new PlanData {
                Name = "Standard Plan",
                Price = "10.99",
                PricePer = "month",
                Features = new List<string> { "10,000 clicks per month", "1,000 meters of mouse wheel usage per month", "Adjustable button mappings", "Basic support" },
                ButtonColor = Color.FromArgb(64, 64, 64), // Darker Grey
                ButtonTextColor = Color.White
            },
            new PlanData {
                Name = "Premium Plan",
                Price = "17.99",
                PricePer = "month",
                Features = new List<string> { "Unlimited clicks", "Unlimited mouse wheel usage", "Custom button mappings", "Priority support" },
                ButtonColor = Color.FromArgb(0, 150, 136),  // Teal
                ButtonTextColor = Color.White,
                HasCrown = true
            }
        };

        public LimitReachedForm(int clickLimit, int unlockTime) {
            _unlockTime = unlockTime;
            _arrowImage = Image.FromFile("assets/arrow.png");

            // Window properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(880, 400); // Initial size, height adjusted later
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.TopMost = true;

            // Title (Header)
            Panel headerPanel = new Panel() {
                Size = new Size(this.Width, 60),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(45, 45, 45)
            };
            Label upgradeRequiredLabel = new Label() {
                Text = "Upgrade Required:",
                ForeColor = Color.White, // Changed to white for readability
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            Label limitReachedLabel = new Label() {
                Text = "Monthly Click Limit Reached",
                ForeColor = Color.FromArgb(120, 120, 120), // Lighter grey
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(upgradeRequiredLabel.Right + 5, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(upgradeRequiredLabel);
            headerPanel.Controls.Add(limitReachedLabel);

            // Subtext
            Label subtext = new Label() {
                Text = $"You have reached the maximum number of clicks allowed ({clickLimit}).\nTo continue using your mouse, please upgrade to a plan.",
                ForeColor = Color.FromArgb(200, 200, 200), // Slightly less white, more light grey
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, headerPanel.Bottom + 10),
                AutoSize = true
            };

            // Subscription cards panel
            Panel subscriptionCardsPanel = new Panel() {
                Size = new Size(580, 260), // Width for two cards, height adjusted later
                Location = new Point(20, subtext.Bottom + 20), // Increased buffer
                BackColor = Color.Transparent
            };

            // Create subscription cards side-by-side
            int cardX = 0;
            int cardSpacing = 10;
            int totalCardHeight = 0;
            foreach (var plan in _plans) {
                Panel card = CreatePlanCard(plan, 0);
                card.Location = new Point(cardX, 0);
                subscriptionCardsPanel.Controls.Add(card);
                cardX += card.Width + cardSpacing;
                totalCardHeight = Math.Max(totalCardHeight, card.Height);
            }
            subscriptionCardsPanel.Height = totalCardHeight;
            subscriptionCardsPanel.Width = cardX - cardSpacing;

            // Mouse image, centered vertically next to cards
            PictureBox mouseImage = new PictureBox() {
                Image = Image.FromFile("assets/mouse.png"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(200, 200),
                Location = new Point(subscriptionCardsPanel.Right + 20, subscriptionCardsPanel.Top + (subscriptionCardsPanel.Height - 200) / 2),
                BackColor = Color.Transparent
            };

            // Remind Me Later button
            Button remindLaterButton = new Button() {
                Text = "Remind Me Later",
                Size = new Size(200, 35),
                Location = new Point(subscriptionCardsPanel.Left + (subscriptionCardsPanel.Width - 200) / 2, Math.Max(subscriptionCardsPanel.Bottom, mouseImage.Bottom) + 20),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            remindLaterButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(remindLaterButton.BackColor, 0.2f);
            remindLaterButton.Click += (s, e) => {
                _unlockTimer = new System.Windows.Forms.Timer { Interval = _unlockTime };
                _unlockTimer.Tick += (sender, args) => {
                    _unlockTimer.Stop();
                    this.Close();
                };
                _unlockTimer.Start();
                remindLaterButton.Text = $"Unlocking in {_unlockTime / 1000}s...";
                remindLaterButton.Enabled = false;
            };

            // Disclaimer label
            var disclaimerLabel = new Label {
                Text = "This is a joke application - use responsibly!",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Size = new Size(320, 30),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls to form
            this.Controls.Add(headerPanel);
            this.Controls.Add(subtext);
            this.Controls.Add(subscriptionCardsPanel);
            this.Controls.Add(mouseImage);
            this.Controls.Add(disclaimerLabel);
            this.Controls.Add(remindLaterButton);

            // Adjust form height and position disclaimer
            this.Height = remindLaterButton.Bottom + 40;
            disclaimerLabel.Location = new Point((this.Width - 320) / 2, this.Height - 35);

            this.FormClosing += (s, e) => _allowClicks = true;
        }

        private Panel CreatePlanCard(PlanData plan, int yOffset) {
            Panel card = new Panel() {
                Size = new Size(280, 120), // Initial size, adjusted later
                Location = new Point(0, yOffset),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            int currentY = 10;
            int xOffset = 10;

            // Crown for premium plan
            if (plan.HasCrown) {
                PictureBox crownImage = new PictureBox() {
                    Image = Image.FromFile("assets/crown.png"),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(20, 20),
                    Location = new Point(xOffset, currentY),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(crownImage);
                xOffset += crownImage.Width + 5;
            }

            // Plan name
            Label planName = new Label() {
                Text = plan.Name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(xOffset, currentY),
                AutoSize = true
            };
            card.Controls.Add(planName);
            currentY = planName.Bottom + 5;

            // Price
            Label priceLabel = new Label() {
                Text = $"${plan.Price}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, currentY),
                AutoSize = true
            };
            card.Controls.Add(priceLabel);

            // Price per label
            Label pricePerLabel = new Label() {
                Text = $"/{plan.PricePer}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), // Larger and bold
                Location = new Point(priceLabel.Right, currentY),
                AutoSize = true
            };
            card.Controls.Add(pricePerLabel);
            currentY = priceLabel.Bottom + 5;

            // Features with arrow images
            int featureX = 10;
            int arrowSize = 16;
            foreach (var feature in plan.Features) {
                PictureBox arrowPb = new PictureBox() {
                    Image = _arrowImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(arrowSize, arrowSize),
                    Location = new Point(featureX, currentY),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(arrowPb);
                Label featureLabel = new Label() {
                    Text = feature,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(featureX + arrowSize + 5, currentY),
                    AutoSize = true
                };
                card.Controls.Add(featureLabel);
                int rowHeight = Math.Max(arrowPb.Height, featureLabel.Height);
                currentY += rowHeight + 5; // Increased spacing
            }

            // Upgrade button
            Button upgradeButton = new Button() {
                Text = $"Upgrade to {plan.Name.Replace(" Plan", "")}",
                Size = new Size(260, 30),
                Location = new Point(10, currentY + 10),
                BackColor = plan.ButtonColor,
                ForeColor = plan.ButtonTextColor,
                FlatStyle = FlatStyle.Flat
            };
            upgradeButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(plan.ButtonColor, 0.2f);
            upgradeButton.Click += (s, e) => {
                MessageBox.Show($"Congratulations! You have subscribed to our {plan.Name}.\nEnjoy your premium mouse experience!", "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };
            card.Controls.Add(upgradeButton);

            // Adjust card height
            card.Height = upgradeButton.Bottom + 10;

            return card;
        }

        protected override void WndProc(ref Message m) {
            const int WM_NCHITTEST = 0x0084;
            const int HTCLIENT = 1;

            if (m.Msg == WM_NCHITTEST && !_allowClicks) {
                m.Result = (IntPtr)HTCLIENT;
                return;
            }
            base.WndProc(ref m);
        }
    }

    public class PlanData {
        public string Name { get; set; }
        public string Price { get; set; }
        public string PricePer { get; set; }
        public List<string> Features { get; set; }
        public Color ButtonColor { get; set; }
        public Color ButtonTextColor { get; set; }
        public bool HasCrown { get; set; } = false;
    }
}