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
            trayMenu.Items.Add("Hello World", null, OnHelloWorld);
            trayMenu.Items.Add("Exit", null, OnExit);
            // Create a tray icon.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Hello World Taskbar App";
            trayIcon.Icon = GenerateIconWithNumber(counter);
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
        private void WorkerThreadFunc()
        {
            var userIncrease = false; //used to keep users changes in instance mem
            var maxBrightness = 80;
            var minBrightness = 0;

            var previouslySet = -999; //detect empty

            while (true)
            {

                //# BRIGHTNESS SET
                //get screen content brightness
                var bitmap = Tools.GetScreenshot();

                //convert to black & white for better white detection
                //var screenshotsMonochrome = Tools.ConvertToMonoschrome(bitmap);

                //var whitePixelPercentage = Tools.GetWhitePixelPercentage(screenshotsMonochrome);
                var whitePixelPercentage = Tools.GetWhitePixelPercentage(bitmap);

                //set brightness based on percentage

                var newBrightness = Tools.GetScreenBrightness();

                var isDarkEnd = whitePixelPercentage < 1.5;
                var isBrightEnd = whitePixelPercentage > 3;
                if (isDarkEnd)
                {
                    Tools.SetScreenBrightness(maxBrightness);
                }
                else if (isBrightEnd)
                {
                    Tools.SetScreenBrightness(minBrightness);
                }

                //set icon in tray
                trayIcon.Icon = GenerateIconWithNumber((int)newBrightness);

                //set extra info for view
                var info =
                    $"White:{Math.Round(whitePixelPercentage, 4)}% | Bright:{Tools.GetScreenBrightness()}% | Max:{maxBrightness} | Min:{minBrightness}";
                Console.WriteLine(info);
                //counterMenuItem.Text = info;

                Thread.Sleep(100); //wait 100ms
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }
        private void OnHelloWorld(object sender, EventArgs e)
        {
            MessageBox.Show("Hello World!");
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