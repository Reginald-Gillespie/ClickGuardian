public class AppSettings{
    public int ClickLimit { get; set; } = 5;
    public int UnlockTime { get; set; } = 5000;
    public bool BlockClicks { get; set; } = true;
    public bool ShowTrayIcon { get; set; } = true; // Default to show tray icon
}
