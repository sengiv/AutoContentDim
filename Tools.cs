using System.Drawing.Imaging;
using System.Management;

namespace AutoContentDim;

public static class Tools
{
    /// <summary>
    /// Remap from 1 range to another
    /// </summary>
    public static double Remap(this double from, double fromMin, double fromMax, double toMin, double toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    // This method requires adding a reference to System.Drawing.Common.dll
    public static Bitmap GetScreenshot()
    {
        // Get the size of the primary screen using the Screen class from System.Windows.Forms
        // This requires adding a reference to System.Windows.Forms.dll as well
        int width = Screen.PrimaryScreen.Bounds.Width;
        int height = Screen.PrimaryScreen.Bounds.Height;

        // Create a bitmap object with the same size as the screen
        Bitmap screenshot = new Bitmap(width, height);

        // Create a graphics object from the bitmap
        Graphics graphics = Graphics.FromImage(screenshot);

        // Copy the screen image to the bitmap
        graphics.CopyFromScreen(0, 0, 0, 0, new Size(width, height));

        // Dispose the graphics object
        graphics.Dispose();

        // Return the bitmap object
        return screenshot;
    }

    public static void SetScreenBrightness(int percentage)
    {
        percentage = percentage < 0 ? 0 : percentage;
        percentage = percentage > 100 ? 100 : percentage;

        ManagementScope scope = new ManagementScope("root\\WMI");
        SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
        {
            using (ManagementObjectCollection objectCollection = searcher.Get())
            {
                foreach (ManagementObject mObj in objectCollection)
                {
                    mObj.InvokeMethod("WmiSetBrightness", new object[] { UInt32.MaxValue, percentage });
                    break;
                }
            }
        }
    }

    public static int GetScreenBrightness()
    {
        ManagementScope scope = new ManagementScope("root\\WMI");
        SelectQuery query = new SelectQuery("WmiMonitorBrightness");
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
        {
            using (ManagementObjectCollection objectCollection = searcher.Get())
            {
                foreach (ManagementObject mObj in objectCollection)
                {
                    return (byte)mObj.Properties["CurrentBrightness"].Value;
                }
            }
        }
        throw new Exception("Unable to get brightness.");
    }

    // This method assumes that the screen is represented by a Bitmap object
    public static double GetWhitePixelPercentage(Bitmap screen)
    {
        // Initialize the variables to store the total and white pixel counts
        int totalPixels = 0;
        int whitePixels = 0;

        // Use unsafe code to access the pixel data directly using pointers
        unsafe
        {
            // Lock the bitmap data in memory
            BitmapData data = screen.LockBits(new System.Drawing.Rectangle(0, 0, screen.Width, screen.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Get the pointer to the first pixel
            byte* ptr = (byte*)data.Scan0;

            // Loop through all the pixels in the bitmap
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    // Increment the total pixel count
                    totalPixels++;

                    // Get the color components of the current pixel
                    byte blue = ptr[0];
                    byte green = ptr[1];
                    byte red = ptr[2];
                    byte alpha = ptr[3];

                    // Check if the pixel is white (all components are 255)
                    if (blue == 255 && green == 255 && red == 255 && alpha == 255)
                    {
                        // Increment the white pixel count
                        whitePixels++;
                    }

                    // Move the pointer to the next pixel
                    ptr += 4;
                }

                // Move the pointer to the next row
                ptr += data.Stride - data.Width * 4;
            }

            // Unlock the bitmap data from memory
            screen.UnlockBits(data);
        }

        // Calculate and return the percentage of white pixels
        return (double)whitePixels / totalPixels * 100;
    }
}