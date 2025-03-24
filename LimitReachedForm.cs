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
        private float _dpiScaleFactor = 1.0f;

        private List<PlanData> _plans = new List<PlanData>() {
            new PlanData {
                Name = "Standard Plan",
                Price = "10.99",
                PricePer = "month",
                Features = new List<string> { "10,000 clicks per month", "1,000 meters of mouse wheel usage per month", "Adjustable button mappings", "Basic support" },
                ButtonColor = Color.FromArgb(64, 64, 64),
                ButtonTextColor = Color.White
            },
            new PlanData {
                Name = "Premium Plan",
                Price = "17.99",
                PricePer = "month",
                Features = new List<string> { "Unlimited clicks", "Unlimited mouse wheel usage", "Custom button mappings", "Priority support" },
                ButtonColor = Color.FromArgb(0, 150, 136),
                ButtonTextColor = Color.White,
                HasCrown = true
            }
        };

        public LimitReachedForm(int clickLimit, int unlockTime) {
            _unlockTime = unlockTime;

            // Set DPI awareness
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Calculate DPI scale factor
            using (Graphics g = this.CreateGraphics()) {
                _dpiScaleFactor = g.DpiX / 96.0f;
            }

            // Load assets
            try {
                _arrowImage = Image.FromFile("assets/arrow.png");
            } catch (Exception) {
                // Fallback if image not found
                _arrowImage = new Bitmap(16, 16);
            }

            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = ScaleSize(new Size(880, 400));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.TopMost = true;

            // Handle DPI changes
            this.DpiChanged += (s, e) => {
                _dpiScaleFactor = e.DeviceDpiNew / 96.0f;
                RebuildUI(clickLimit);
            };

            // Build the UI
            RebuildUI(clickLimit);

            this.FormClosing += (s, e) => _allowClicks = true;
        }

        private void RebuildUI(int clickLimit) {
            // Clear existing controls
            this.Controls.Clear();

            // Header
            Panel headerPanel = new Panel() {
                Size = ScaleSize(new Size(this.Width, 60)),
                Location = ScalePoint(new Point(0, 0)),
                BackColor = Color.FromArgb(45, 45, 45)
            };

            Font headerFont = new Font("Segoe UI", ScaleFont(14), FontStyle.Bold);
            Label upgradeRequiredLabel = new Label() {
                Text = "Upgrade Required:",
                ForeColor = Color.White,
                Font = headerFont,
                AutoSize = true
            };

            Label limitReachedLabel = new Label() {
                Text = "Monthly Click Limit Reached",
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = headerFont,
                AutoSize = true
            };

            headerPanel.Controls.Add(upgradeRequiredLabel);
            headerPanel.Controls.Add(limitReachedLabel);

            // Center the title text
            upgradeRequiredLabel.Location = new Point(10, (headerPanel.Height - upgradeRequiredLabel.Height) / 2);
            limitReachedLabel.Location = new Point(upgradeRequiredLabel.Right + ScaleSize(10),
                                                  (headerPanel.Height - limitReachedLabel.Height) / 2);

            // Subtext
            Label subtext = new Label() {
                Text = $"You have reached the maximum number of clicks allowed ({clickLimit}).\nTo continue using your mouse, please upgrade to a plan.",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", ScaleFont(9)),
                Location = new Point(20, headerPanel.Bottom + ScaleSize(20)),
                AutoSize = true
            };

            // Subscription cards panel
            Panel subscriptionCardsPanel = new Panel() {
                Location = new Point(20, subtext.Bottom + ScaleSize(10)),
                BackColor = Color.Transparent
            };

            // Add plan cards
            int cardX = 0;
            int cardSpacing = ScaleSize(10);
            int totalCardHeight = 0;

            foreach (var plan in _plans) {
                Panel card = CreatePlanCard(plan);
                card.Location = new Point(cardX, 0);
                subscriptionCardsPanel.Controls.Add(card);
                cardX += card.Width + cardSpacing;
                totalCardHeight = Math.Max(totalCardHeight, card.Height);
            }

            subscriptionCardsPanel.Size = new Size(cardX - cardSpacing, totalCardHeight);

            // Mouse image
            PictureBox mouseImage = new PictureBox() {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = ScaleSize(new Size(200, 200)),
                BackColor = Color.Transparent
            };

            try {
                mouseImage.Image = Image.FromFile("assets/mouse.png");
            } catch (Exception) {
                // Create a simple placeholder if image not found
                Bitmap placeholder = new Bitmap(mouseImage.Width, mouseImage.Height);
                using (Graphics g = Graphics.FromImage(placeholder)) {
                    g.Clear(Color.DarkGray);
                }
                mouseImage.Image = placeholder;
            }

            mouseImage.Location = new Point(
                subscriptionCardsPanel.Right + ScaleSize(20),
                subscriptionCardsPanel.Top + (subscriptionCardsPanel.Height - mouseImage.Height) / 2
            );

            // Remind Me Later button
            Button remindLaterButton = new Button() {
                Text = "Remind Me Later",
                Size = ScaleSize(new Size(200, 35)),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9))
            };

            remindLaterButton.Location = new Point(
                subscriptionCardsPanel.Left + (subscriptionCardsPanel.Width - remindLaterButton.Width) / 2,
                Math.Max(subscriptionCardsPanel.Bottom, mouseImage.Bottom) + ScaleSize(20)
            );

            remindLaterButton.FlatAppearance.BorderSize = 0;
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

            // Add all controls
            this.Controls.Add(headerPanel);
            this.Controls.Add(subtext);
            this.Controls.Add(subscriptionCardsPanel);
            this.Controls.Add(mouseImage);
            this.Controls.Add(remindLaterButton);

            // Resize form to fit all controls
            // this.Height = remindLaterButton.Bottom + ScaleSize(40);
            this.Height = remindLaterButton.Bottom + ScaleSize(20);

            // Ensure width is sufficient for all elements
            this.Width = Math.Max(this.Width, mouseImage.Right + ScaleSize(20));
        }

        private Panel CreatePlanCard(PlanData plan) {
            Panel card = new Panel() {
                Size = ScaleSize(new Size(280, 240)), // Start with a height that we'll adjust
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.None
            };

            int currentY = ScaleSize(10);
            int xOffset = ScaleSize(10);

            if (plan.HasCrown) {
                PictureBox crownImage = new PictureBox() {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = ScaleSize(new Size(20, 20)),
                    Location = new Point(xOffset, currentY),
                    BackColor = Color.Transparent
                };

                try {
                    crownImage.Image = Image.FromFile("assets/crown.png");
                } catch (Exception) {
                    // Create a simple crown placeholder if image not found
                    Bitmap crownPlaceholder = new Bitmap(crownImage.Width, crownImage.Height);
                    using (Graphics g = Graphics.FromImage(crownPlaceholder)) {
                        g.Clear(Color.Gold);
                    }
                    crownImage.Image = crownPlaceholder;
                }

                card.Controls.Add(crownImage);
                xOffset += crownImage.Width + ScaleSize(5);
            }

            Label planName = new Label() {
                Text = plan.Name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", ScaleFont(10), FontStyle.Bold),
                Location = new Point(xOffset, currentY),
                AutoSize = true
            };
            card.Controls.Add(planName);
            currentY = planName.Bottom + ScaleSize(5);

            Label priceLabel = new Label() {
                Text = $"${plan.Price}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", ScaleFont(20), FontStyle.Bold),
                Location = new Point(ScaleSize(10), currentY),
                AutoSize = true
            };
            card.Controls.Add(priceLabel);

            Label pricePerLabel = new Label() {
                Text = $"/{plan.PricePer}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", ScaleFont(10)),
                Location = new Point(priceLabel.Right, priceLabel.Top + ScaleSize(4)), // Align with the price
                AutoSize = true
            };
            card.Controls.Add(pricePerLabel);
            currentY = priceLabel.Bottom + ScaleSize(10);

            int featureX = ScaleSize(10);
            int arrowSize = ScaleSize(16);

            foreach (var feature in plan.Features) {
                PictureBox arrowPb = new PictureBox() {
                    Image = _arrowImage,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(arrowSize, arrowSize),
                    Location = new Point(featureX, currentY + ScaleSize(2)), // Vertically center with text
                    BackColor = Color.Transparent
                };
                card.Controls.Add(arrowPb);

                Label featureLabel = new Label() {
                    Text = feature,
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", ScaleFont(9)),
                    Location = new Point(featureX + arrowSize + ScaleSize(5), currentY),
                    AutoSize = true,
                    MaximumSize = new Size(card.Width - featureX - arrowSize - ScaleSize(15), 0) // Allow wrapping
                };
                card.Controls.Add(featureLabel);

                currentY += featureLabel.Height + ScaleSize(8);
            }

            Button upgradeButton = new Button() {
                Text = $"Upgrade to {plan.Name.Replace(" Plan", "")}",
                Size = new Size(card.Width - ScaleSize(20), ScaleSize(30)),
                Location = new Point(ScaleSize(10), currentY + ScaleSize(10)),
                BackColor = plan.ButtonColor,
                ForeColor = plan.ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9))
            };

            upgradeButton.FlatAppearance.BorderSize = 0;
            upgradeButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(plan.ButtonColor, 0.2f);

            upgradeButton.Click += (s, e) => {
                MessageBox.Show($"Congratulations! You have subscribed to our {plan.Name}.\nEnjoy your premium mouse experience!",
                                "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            card.Controls.Add(upgradeButton);

            // Adjust card height to fit content
            card.Height = upgradeButton.Bottom + ScaleSize(10);

            return card;
        }

        // Helper methods for DPI scaling
        private int ScaleSize(int size) {
            return (int)(size * _dpiScaleFactor);
        }

        private Size ScaleSize(Size size) {
            return new Size(
                (int)(size.Width * _dpiScaleFactor),
                (int)(size.Height * _dpiScaleFactor)
            );
        }

        private Point ScalePoint(Point point) {
            return new Point(
                (int)(point.X * _dpiScaleFactor),
                (int)(point.Y * _dpiScaleFactor)
            );
        }

        private float ScaleFont(float size) {
            // Font scaling is a bit different - we don't want to multiply
            // directly by DPI scale factor as it can make text too large
            return size; // .NET will handle font scaling appropriately
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