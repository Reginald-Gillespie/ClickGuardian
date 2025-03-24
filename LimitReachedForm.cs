using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using SkiaSharp;
using Svg.Skia; // Note: using Svg.Skia instead of SkiaSharp.Extended.Svg
using SkiaSharp.Views.Desktop; // For Extensions.ToBitmap

namespace ClickLimiter {
    public class LimitReachedForm : Form {
        private System.Windows.Forms.Timer _unlockTimer;
        private int _remainingTime;
        private bool _allowClicks = false;
        private int _unlockTime;
        private Image _arrowImage;
        private float _dpiScaleFactor = 1.0f;
        private Button _remindLaterButton;
        public event Action OnLimitReset;

        private Image LoadSvgAsBitmap(string filePath, int width, int height) {
            try {
                // Check if file exists first
                if (!File.Exists(filePath)) {
                    return CreatePlaceholderBitmap(width, height, "File not found");
                }

                using (var stream = File.OpenRead(filePath)) {
                    var svg = new SKSvg();
                    svg.Load(stream);

                    // Check if SVG loaded successfully
                    if (svg.Picture == null) {
                        return CreatePlaceholderBitmap(width, height, "SVG is invalid");
                    }
                    if (svg.Picture.CullRect.IsEmpty) {
                        return CreatePlaceholderBitmap(width, height, "SVG is empty");
                    }

                    // Render the SVG
                    SKRect bounds = svg.Picture.CullRect;
                    float scale = Math.Min((float)width / bounds.Width, (float)height / bounds.Height);
                    var skBitmap = new SKBitmap(width, height);
                    using (var canvas = new SKCanvas(skBitmap)) {
                        canvas.Clear(SKColors.Transparent);
                        float translateX = (width - bounds.Width * scale) / 2f;
                        float translateY = (height - bounds.Height * scale) / 2f;
                        canvas.Translate(translateX, translateY);
                        canvas.Scale(scale);
                        canvas.DrawPicture(svg.Picture);
                    }
                    using (var skImage = SKImage.FromBitmap(skBitmap)) {
                        return skImage.ToBitmap();
                    }
                }
            } catch (Exception ex) {
                return CreatePlaceholderBitmap(width, height, $"Error: {ex.Message}");
            }
        }

        // Helper method to create a visible placeholder
        private Bitmap CreatePlaceholderBitmap(int width, int height, string message) {
            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(placeholder)) {
                g.Clear(Color.DarkGray);
                // Optional: Add text for debugging
                using (Font font = new Font("Arial", 10))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) {
                    g.DrawString(message, font, Brushes.White, new RectangleF(0, 0, width, height), sf);
                }
            }
            return placeholder;
        }
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
                ButtonColor = Color.FromArgb(0, 200, 180),
                ButtonTextColor = Color.White,
                HasCrown = true
            }
        };

        public LimitReachedForm(int clickLimit, int unlockTime) {
            _unlockTime = unlockTime;
            _remainingTime = unlockTime / 1000;

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
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.TopMost = true;

            // Force center
            // this.StartPosition = FormStartPosition.CenterScreen;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2
            );
            this.Icon = new Icon("assets/tray.ico"); // Set window icon


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
            limitReachedLabel.Location = new Point(upgradeRequiredLabel.Right + ScaleSize(5),
                                                  (headerPanel.Height - limitReachedLabel.Height) / 2);

            // Subtext
            Label subtext = new Label() {
                Text = $"You have reached the maximum number of clicks allowed ({clickLimit}) for this month.\nTo continue using your mouse without interruption, please upgrade to a monthly subscription.",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", ScaleFont(9)),
                Location = new Point(20, headerPanel.Bottom + ScaleSize(20)),
                AutoSize = true
            };

            // Fix #1: Ensure consistent spacing regardless of DPI scaling
            // Add more buffer space between the text and plan cards
            int subscriptionPanelY = subtext.Bottom + ScaleSize(30);

            // Subscription cards panel
            Panel subscriptionCardsPanel = new Panel() {
                Location = new Point(20, subscriptionPanelY),
                BackColor = Color.Transparent
            };

            // Create the plan cards first to determine the tallest card
            List<Panel> cardPanels = new List<Panel>();
            List<Button> upgradeButtons = new List<Button>();
            int maxCardContentHeight = 0;

            foreach (var plan in _plans) {
                // Create the card but don't set final height yet
                Panel card = CreatePlanCard(plan, out Button upgradeButton, out int contentHeight);
                cardPanels.Add(card);
                upgradeButtons.Add(upgradeButton);
                maxCardContentHeight = Math.Max(maxCardContentHeight, contentHeight);
            }

            // Add plan cards with equal height and aligned buttons
            int cardX = 0;
            int cardWidth = ScaleSize(280);
            int cardSpacing = ScaleSize(10);
            int buttonHeight = ScaleSize(30);
            int buttonMargin = ScaleSize(10);

            // Calculate total card height including button and margins
            int totalCardHeight = maxCardContentHeight + buttonHeight + (2 * buttonMargin);

            for (int i = 0; i < cardPanels.Count; i++) {
                Panel card = cardPanels[i];
                Button button = upgradeButtons[i];

                // Set the same total height for all cards
                card.Height = totalCardHeight;
                card.Width = cardWidth;
                card.Location = new Point(cardX, 0);

                // Fix #2: Position the button at the bottom of the card
                button.Location = new Point(
                    buttonMargin,
                    totalCardHeight - buttonHeight - buttonMargin
                );

                subscriptionCardsPanel.Controls.Add(card);
                cardX += card.Width + cardSpacing;
            }

            subscriptionCardsPanel.Size = new Size(cardX - cardSpacing, totalCardHeight);

            // Remind Me Later button
            _remindLaterButton = new Button() {
                Text = "Remind Me Later",
                Size = ScaleSize(new Size(200, 35)),
                BackColor = Color.FromArgb(64, 64, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9))
            };

            _remindLaterButton.FlatAppearance.BorderSize = 0;
            _remindLaterButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(_remindLaterButton.BackColor, 0.2f);

            _remindLaterButton.Click += (s, e) => {
                _unlockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
                int remainingTime = _unlockTime / 1000;
                _unlockTimer.Tick += (sender, args) => {
                    if (remainingTime > 0) {
                        _remindLaterButton.Text = $"Unlocking in {remainingTime}s...";
                        _remindLaterButton.ForeColor = Color.White; // Ensure text remains visible
                        remainingTime--;
                    } else {
                        _unlockTimer.Stop();
                        OnLimitReset?.Invoke();
                        this.Close();
                    }
                };
                _unlockTimer.Start();
                _remindLaterButton.Text = $"Unlocking in {_unlockTime / 1000}s...";
                _remindLaterButton.ForeColor = Color.White; // Ensure initial color
                _remindLaterButton.Enabled = false;
            };

            // Mouse image - Increased size
            PictureBox mouseImage = new PictureBox() {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = ScaleSize(new Size(300, 300)), // Larger mouse image
                BackColor = Color.Transparent
            };

            try {
                mouseImage.Image = LoadSvgAsBitmap("assets/mouse.svg", ScaleSize(300), ScaleSize(300));
            } catch (Exception) {
                // Create a simple placeholder if image not found
                Bitmap placeholder = new Bitmap(mouseImage.Width, mouseImage.Height);
                using (Graphics g = Graphics.FromImage(placeholder)) {
                    g.Clear(Color.DarkGray);
                }
                mouseImage.Image = placeholder;
            }

            // Position remind later button
            _remindLaterButton.Location = new Point(
                subscriptionCardsPanel.Left + (subscriptionCardsPanel.Width - _remindLaterButton.Width) / 2,
                subscriptionCardsPanel.Bottom + ScaleSize(20)
            );

            // Position mouse image to take up the full right side
            int mouseY = headerPanel.Bottom + ScaleSize(20);
            int mouseHeight = _remindLaterButton.Bottom - mouseY;
            mouseImage.Size = new Size(ScaleSize(300), mouseHeight);

            // Center mouse image on right side with less buffer space
            int availableRightSpace = this.Width - subscriptionCardsPanel.Right - ScaleSize(20);
            mouseImage.Location = new Point(
                subscriptionCardsPanel.Right + ScaleSize(20) + (availableRightSpace - mouseImage.Width) / 2,
                mouseY + (mouseHeight - mouseImage.Height) / 2
            );

            // Add all controls
            this.Controls.Add(headerPanel);
            this.Controls.Add(subtext);
            this.Controls.Add(subscriptionCardsPanel);
            this.Controls.Add(mouseImage);
            this.Controls.Add(_remindLaterButton);

            // Resize form to fit all controls
            this.Height = _remindLaterButton.Bottom + ScaleSize(20);

            // Ensure width is sufficient for all elements
            this.Width = Math.Max(this.Width, mouseImage.Right + ScaleSize(20));
        }

        private Panel CreatePlanCard(PlanData plan, out Button upgradeButton, out int contentHeight) {
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
                AutoSize = true
            };
            card.Controls.Add(pricePerLabel);

            // Position at the bottom of the price label instead of at the top
            pricePerLabel.Location = new Point(
                priceLabel.Right,
                priceLabel.Bottom - pricePerLabel.Height - ScaleSize(2)
            );

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

            // Save the content height before adding the button
            contentHeight = currentY;

            // Create the upgrade button but don't position it yet
            upgradeButton = new Button() {
                Text = $"Upgrade to {plan.Name.Replace(" Plan", "")}",
                Size = new Size(card.Width - ScaleSize(20), ScaleSize(30)),
                BackColor = plan.ButtonColor,
                ForeColor = plan.ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9))
            };

            upgradeButton.FlatAppearance.BorderSize = 0;
            upgradeButton.FlatAppearance.MouseOverBackColor = ControlPaint.Light(plan.ButtonColor, 0.2f);

            upgradeButton.Click += (s, e) => {
                // MessageBox.Show($"Congratulations! You have subscribed to our {plan.Name}.\nEnjoy your premium mouse experience!",
                //                 "Subscription Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start("https://example.com/");
                OnLimitReset?.Invoke();
                this.Close();
            };

            card.Controls.Add(upgradeButton);

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