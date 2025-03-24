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

        // Centralized plan data
        private List<PlanData> _plans = new List<PlanData>() {
            new PlanData {
                Name = "Standard Plan",
                Price = "10.99",
                PricePer = "month",
                Features = new List<string> { "10,000 clicks per month", "Adjustable button mappings" },
                ButtonColor = Color.FromArgb(64, 64, 64), // Darker Grey
                ButtonTextColor = Color.White
            },
            new PlanData {
                Name = "Premium Plan",
                Price = "17.99",
                PricePer = "month",
                Features = new List<string> { "Unlimited clicks", "Custom button mappings", "Priority support" },
                ButtonColor = Color.FromArgb(0, 150, 136),  // Teal
                ButtonTextColor = Color.White,
                HasCrown = true
            }
        };

        public LimitReachedForm(int clickLimit, int unlockTime) {
            _unlockTime = unlockTime;

            // Window properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(880, 400); // Increased initial width to accommodate side-by-side cards, adjust height later
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.TopMost = true;

            // Title (Header)
            Panel headerPanel = new Panel() {
                Size = new Size(this.Width, 60),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(45, 45, 45) // Slightly lighter than the background
            };
            Label upgradeRequiredLabel = new Label() {
                Text = "Upgrade Required:",
                ForeColor = Color.Black,  // "Upgrade Required" in black
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label limitReachedLabel = new Label() {
                Text = "Monthly Click Limit Reached",
                ForeColor = Color.FromArgb(80, 80, 80), // Slightly greyer black
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(upgradeRequiredLabel.Right + 5, 15), // Position next to "Upgrade Required"
                AutoSize = true
            };
            headerPanel.Controls.Add(upgradeRequiredLabel);
            headerPanel.Controls.Add(limitReachedLabel);

            // Subtext
            Label subtext = new Label() {
                Text = $"You have reached the maximum number of clicks allowed ({clickLimit}).\nTo continue using your mouse, please upgrade to a plan.",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                Location = new Point(20, headerPanel.Bottom + 10), // Position below the header
                AutoSize = true
            };

            // Panel to hold subscription cards
            Panel subscriptionCardsPanel = new Panel() {
                Size = new Size(580, 260), // Increased width to hold two cards side-by-side, adjust height dynamically below
                Location = new Point(20, subtext.Bottom + 10),
                BackColor = Color.Transparent
            };

            // Dynamically create subscription cards side-by-side
            int cardX = 0;
            int cardSpacing = 10; // Spacing between cards
            int totalCardHeight = 0;
            foreach (var plan in _plans) {
                Panel card = CreatePlanCard(plan, 0); // yOffset is now 0 as cards are side-by-side within the panel
                card.Location = new Point(cardX, 0); // Position horizontally
                subscriptionCardsPanel.Controls.Add(card);
                cardX += card.Width + cardSpacing; // Increment X position for the next card
                totalCardHeight = Math.Max(totalCardHeight, card.Height); // Keep track of the max height
            }
            subscriptionCardsPanel.Height = totalCardHeight; // Set panel height to the max card height
            subscriptionCardsPanel.Width = cardX - cardSpacing; // Adjust panel width to fit cards and spacing

            // Mouse Image - Centered Vertically to the right of cards
            PictureBox mouseImage = new PictureBox() {
                Image = Image.FromFile("assets/mouse.png"), // Replace "mouse.png" with your actual file path
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(200, 200),
                Location = new Point(subscriptionCardsPanel.Right + 20, subtext.Bottom), // Initial X, Y
                BackColor = Color.Transparent
            };
            mouseImage.Location = new Point(subscriptionCardsPanel.Right + 20, subscriptionCardsPanel.Top + (subscriptionCardsPanel.Height - mouseImage.Height) / 2); // Center vertically relative to cards


            // "Remind Me Later" button (below subscriptionCardsPanel and mouseImage), centered below cards
            Button remindLaterButton = new Button() {
                Text = "Remind Me Later",
                Size = new Size(200, 35),
                Location = new Point(subscriptionCardsPanel.Left + (subscriptionCardsPanel.Width - 200) / 2, Math.Max(subscriptionCardsPanel.Bottom, mouseImage.Bottom) + 20), // Position below both panels, centered horizontally relative to cards
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            remindLaterButton.Click += (s, e) => {
                // Start timer to re-enable clicks
                _unlockTimer = new System.Windows.Forms.Timer { Interval = _unlockTime };
                _unlockTimer.Tick += (sender, args) => {
                    _unlockTimer.Stop();
                    this.Close();
                };
                _unlockTimer.Start();

                remindLaterButton.Text = $"Unlocking in {_unlockTime / 1000}s...";
                remindLaterButton.Enabled = false;
            };

            // Add a warning for the joke
            var disclaimerLabel = new Label {
                Text = "This is a joke application - use responsibly!",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(40, this.Height - 25),
                Size = new Size(320, 30),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls to the form
            this.Controls.Add(headerPanel);
            this.Controls.Add(subtext);
            this.Controls.Add(subscriptionCardsPanel);
            this.Controls.Add(mouseImage);
            this.Controls.Add(disclaimerLabel);
            this.Controls.Add(remindLaterButton);

            // Adjust form height to fit all content
            this.Height = remindLaterButton.Bottom + 40; // Add some extra padding at the bottom


            this.FormClosing += (s, e) => _allowClicks = true; // Allow normal behavior on close
        }

        // Method to create individual plan cards
        private Panel CreatePlanCard(PlanData plan, int yOffset) { // yOffset is no longer used for vertical stacking, but kept for method signature
            Panel card = new Panel() {
                Size = new Size(280, 120), // Initial size, will be adjusted
                Location = new Point(0, yOffset), // yOffset is now always 0 for side-by-side layout within the panel
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            int currentY = 10;
            int xOffset = 10;

            // Crown (if applicable)
            if (plan.HasCrown) {
                PictureBox crownImage = new PictureBox() {
                    Image = Image.FromFile("assets/crown.png"), // Replace "crown.png" with your actual path
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(20, 20),
                    Location = new Point(xOffset, currentY),
                    BackColor = Color.Transparent
                };
                card.Controls.Add(crownImage);
                xOffset += crownImage.Width + 5;
            }

            // Plan Name
            Label planName = new Label() {
                Text = plan.Name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(xOffset, currentY),
                AutoSize = true
            };
            card.Controls.Add(planName);
            currentY = planName.Bottom + 5;

            // Price (larger font)
            Label priceLabel = new Label() {
                Text = $"${plan.Price}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, currentY),
                AutoSize = true
            };
            card.Controls.Add(priceLabel);

            // per month/year label
            Label pricePerLabel = new Label() {
                Text = $"/{plan.PricePer}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Location = new Point(priceLabel.Right, currentY + 5), // Adjusted Y position
                AutoSize = true
            };
            card.Controls.Add(pricePerLabel);
            currentY = priceLabel.Bottom + 5;


            // Features
            int featureStartY = currentY; // Keep track of the starting Y for features
            foreach (var feature in plan.Features) {
                Label featureLabel = new Label() {
                    Text = "• " + feature,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(10, currentY),
                    AutoSize = true
                };
                card.Controls.Add(featureLabel);
                currentY = featureLabel.Bottom; // Increment currentY after each feature
            }

            // Adjust card height based on features and add some padding
            card.Height = currentY - card.Location.Y + 45; // Calculate height based on last feature label + button height + padding

            // Upgrade Button - position at the bottom of the card
            Button upgradeButton = new Button() {
                Text = $"Upgrade to {plan.Name.Replace(" Plan", "")}",
                Size = new Size(260, 30),
                Location = new Point(10, card.Height - 40), // Position at the bottom of the card, now dynamically sized
                BackColor = plan.ButtonColor,
                ForeColor = plan.ButtonTextColor,
                FlatStyle = FlatStyle.Flat
            };
            upgradeButton.Click += (s, e) => {
                MessageBox.Show($"Congratulations! You have subscribed to our {plan.Name}.\nEnjoy your premium mouse experience!", "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };
            card.Controls.Add(upgradeButton);

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

    // Data structure for plan information
    public class PlanData {
        public string Name { get; set; }
        public string Price { get; set; }
        public string PricePer { get; set; }
        public List<string> Features { get; set; }
        public Color ButtonColor { get; set; }
        public Color ButtonTextColor { get; set; }
        public bool HasCrown { get; set; } = false; // Default to no crown
    }
}