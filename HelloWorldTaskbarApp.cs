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
            FreeConsole();
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
            var maxBrightness = 30;
            var minBrightness = 0;

            var previouslySet = -999; //detect empty

            while (true)
            {

                //# BRIGHTNESS SET
                //get screen content brightness
                var bitmap = Tools.GetScreenshot();
                var whitePixelPercentage = Tools.GetWhitePixelPercentage(bitmap);

                //set brightness based on percentage
                // Calculate scale factor
                double range = maxBrightness - minBrightness;
                double scaleFactor = range / 0.3;
                // Set brightness based on percentage
                // Use a linear function that maps 0.3 to minBrightness, and 0 to maxBrightness
                var newBrightness = (whitePixelPercentage - 0.3) * (-scaleFactor) + minBrightness;
                //clamp the brightness value between 0 and 40
                newBrightness = Math.Max(minBrightness, Math.Min(maxBrightness, newBrightness));

                //detect if user changed brightness and auto change the max & min limit
                //based on increase or decrease while sleeping
                var nowBright = Tools.GetScreenBrightness();
                previouslySet = previouslySet == -999 ? nowBright : previouslySet;//on 1st run no previous to check so skip

                int threshold = 5; // Set your threshold value
                if (Math.Abs(nowBright - previouslySet) > threshold) // Check if the difference is greater than the threshold
                {
                    if (nowBright > previouslySet) // User increased
                    {
                        if (previouslySet == 0 || nowBright == 0 || userIncrease)
                        {
                            minBrightness += 10;
                            userIncrease = !userIncrease; //only set if coming from 0
                        }
                        else
                        {
                            maxBrightness += 10;
                            userIncrease = false;
                        }
                    }
                    else if (nowBright < previouslySet) // User decreased
                    {
                        if (previouslySet == 100)
                        {
                            minBrightness -= 10;
                        }
                        else
                        {
                            maxBrightness -= 10;
                        }
                    }

                }

                previouslySet = (int)newBrightness; //update previous
                Tools.SetScreenBrightness((int)newBrightness);

                //set icon in tray
                trayIcon.Icon = GenerateIconWithNumber(previouslySet);

                //set extra info for view
                //var info =
                //    $"White:{Math.Round(whitePixelPercentage, 4)}% | Bright:{Tools.GetScreenBrightness()}% | Max:{maxBrightness} | Min:{minBrightness}";
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