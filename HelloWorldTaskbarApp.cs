using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoContentDim
{
    public class HelloWorldTaskbarApp : Form
    {
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private Thread workerThread;
        private int counter = 0;
        private ToolStripMenuItem counterMenuItem;
        public HelloWorldTaskbarApp()
        {
            // Hide console window
            // FreeConsole();
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenuStrip();
            counterMenuItem = new ToolStripMenuItem(counter.ToString());
            trayMenu.Items.Add(counterMenuItem);
            trayMenu.Items.Add("+ Min Threshold", null, (sender, args) => MinThreshold += 0.1);
            trayMenu.Items.Add("- Min Threshold", null, (sender, args) => MinThreshold -= 0.1);
            trayMenu.Items.Add("+ Max Threshold", null, (sender, args) => MinThreshold += 0.1);
            trayMenu.Items.Add("- Max Threshold", null, (sender, args) => MinThreshold -= 0.1);
            trayMenu.Items.Add("+ Min", null, (sender, args) => minBrightness += 10);
            trayMenu.Items.Add("- Min", null, (sender, args) => minBrightness -= 10);
            trayMenu.Items.Add("+ Max", null, (sender, args) => maxBrightness += 10);
            trayMenu.Items.Add("- Max", null, (sender, args) => maxBrightness -= 10);

            trayMenu.Items.Add("Exit", null, OnExit);
            // Create a tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Auto Content Dimmer";
            trayIcon.Icon =  GenerateIconWithNumber(counter);
            // Add menu to tray icon and show it.
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            // Start the worker thread
            workerThread = new Thread(WorkerThreadFunc);
            workerThread.Start();
        }
        private Icon GenerateIconWithNumber(int number)
        {
            Bitmap bitmap = new Bitmap(16, 16);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawString(number.ToString(), SystemFonts.DefaultFont, Brushes.Bisque, 0, 0);
            }
            Icon icon = Icon.FromHandle(bitmap.GetHicon());
            return icon;
        }

        private double MinThreshold = 1.5;
        private double MaxThreshold = 5;
        private int maxBrightness = 50;
        private int minBrightness = 10;

        private void WorkerThreadFunc()
        {
            while (true)
            {
                // Get screen content brightness
                var bitmap = Tools.GetScreenshot();
                // Get white pixel percentage
                var whitePixelPercentage = Tools.GetWhitePixelPercentage(bitmap);
                // Any white pixel percentage above 60 is considered as maximum (100)
                whitePixelPercentage = whitePixelPercentage > 60 ? 100 : whitePixelPercentage;
                // Calculate new brightness based on white pixel percentage
                var newBrightness = maxBrightness - ((maxBrightness - minBrightness) * (whitePixelPercentage / 100));
                // Set brightness
                Tools.SetScreenBrightness((int)newBrightness);
                // Set extra info for view
                var info =
                    $"White:{Math.Round(whitePixelPercentage, 4)}% | Bright:{Tools.GetScreenBrightness()}% | Max:{maxBrightness} | Min:{minBrightness}";
                Console.WriteLine(info);
                Thread.Sleep(100); // Wait 100ms
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }
        
        private void OnExit(object sender, EventArgs e)
        {
            // Stop the worker thread

            Application.Exit();
        }
        
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

    }

}