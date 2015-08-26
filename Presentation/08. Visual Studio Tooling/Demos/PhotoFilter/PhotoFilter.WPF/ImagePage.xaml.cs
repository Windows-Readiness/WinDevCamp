using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PhotoFilter.WPF
{
    struct PixelProcessingResults
    {
        public bool Success;
        public byte[] Pixels;

        public PixelProcessingResults(bool success)
        {
            this.Success = success;
            this.Pixels = null;
        }

        public PixelProcessingResults(bool success, byte[] pixels)
        {
            this.Success = success;
            this.Pixels = pixels;
        }
    }

    public class PhotoFilterNativeMethods
    {
        [DllImport("PhotoFilterLib.Win32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ApplyAntiqueFilter(IntPtr pixels, int size);
    }

    /// <summary>
    /// Interaction logic for ImagePage.xaml
    /// </summary>
    public partial class ImagePage : Window
    {
        ListView _parentList;
        BitmapSource _image;
        bool _initialiazed = false;
        public ImagePage(ListView parentList)
        {
            _parentList = parentList;
            InitializeComponent();
            UpdateImageFromParent();
            _parentList.SelectionChanged += _parentList_SelectionChanged;
        }

        private void UpdateImageFromParent()
        {
            ImageItem item = _parentList.SelectedItem as ImageItem;
            if (item == null) { return; }
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = item.Uri;
            image.EndInit();
            _image = image;
            FullImage.Source = _image;
        }

        protected override void OnClosed(EventArgs e)
        {
            //_parentList.SelectionChanged -= _parentList_SelectionChanged;
            base.OnClosed(e);
        }

        void _parentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateImageFromParent();
        }

        private async void FilterImageParallel_Click(object sender, RoutedEventArgs e)
        {
            await ApplyImageFilter(parallel: true);
        }

        private async void FilterImage_Click(object sender, RoutedEventArgs e)
        {
            await ApplyImageFilter(parallel: false);
        }

        private async Task ApplyImageFilter(bool parallel)
        {
            int stride = _image.PixelWidth * 4;

            lblError.Visibility = Visibility.Collapsed;

            //Do the processing on a background thread so the UI remains responsive
            var processingResults = await Task<PixelProcessingResults>.Run(() =>
            {
                PixelProcessingResults result;

                try {
                    byte[] pixelArray = new byte[_image.PixelHeight * _image.PixelWidth * 4];
                    _image.CopyPixels(pixelArray, stride, 0);

                    if (parallel == true)
                    {
                        pixelArray = ApplyAntiqueFilterParallel(pixelArray);
                    }
                    else
                    {
                        pixelArray = ApplyAntiqueFilter(pixelArray);
                    }

                    result = new PixelProcessingResults(true, pixelArray);
                }
                catch (Exception)
                {
                    result = new PixelProcessingResults(false);
                }

                return result;
            });

            if (processingResults.Success)
            {
                _image = BitmapSource.Create(_image.PixelWidth, _image.PixelHeight, _image.DpiX, _image.DpiY,
                        PixelFormats.Bgr32, null, processingResults.Pixels, stride);
                FullImage.Source = _image;
            }
            else
            {
                lblError.Visibility = Visibility.Visible;
            }

        }

        private byte[] ApplyAntiqueFilter(byte[] buffer)
        {
            var pixels = new byte[buffer.Length];

            for (int x = 0; x < buffer.Length; x += 4)
            {
                for (int n = 0; n < 8; ++n)
                {
                    int rgincrease = 100;
                    int red = buffer[x] + rgincrease;
                    int green = buffer[x + 1] + rgincrease;
                    int blue = buffer[x + 2];
                    int alpha = buffer[x + 3];

                    if (red > 255)
                        red = 255;
                    if (green > 255)
                        green = 255;

                    pixels[x] = (byte)red;
                    pixels[x + 1] = (byte)green;
                    pixels[x + 2] = (byte)blue;
                    pixels[x + 3] = (byte)alpha;
                }
            }
            return pixels;
        }

        private byte[] ApplyAntiqueFilterParallel(byte[] buffer)
        {
            var pixels = new byte[buffer.Length];

            int iterations = 1000;
            int chunkSize = buffer.Length / iterations;
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 4;
            Parallel.For(0, iterations, (int i) =>
            {
                for (int n = 0; n < 8; ++n)
                {
                    int lower = i * chunkSize;
                    int upper = lower + chunkSize;
                    int startOffset = i * 4;
                    int endOffset = iterations * 4 - startOffset;
                    for (int x = lower; x < upper; x += 4)
                    {
                        int rgincrease = 100;
                        int red = buffer[x] + rgincrease;
                        int green = buffer[x + 1] + rgincrease;
                        int blue = buffer[x + 2];
                        int alpha = buffer[x + 3];

                        if (red > 255)
                            red = 255;
                        if (green > 255)
                            green = 255;

                        pixels[x] = (byte)red;
                        pixels[x + 1] = (byte)green;
                        pixels[x + 2] = (byte)blue;
                        pixels[x + 3] = (byte)alpha;
                    }
                }
            });
            return pixels;
        }

        private byte[] ApplyAntiqueFilterNative(byte[] pixelArray)
        {
            int size = Marshal.SizeOf(pixelArray[0]) * pixelArray.Length;

            IntPtr pixelsToNative = Marshal.AllocHGlobal(size);
            Marshal.Copy(pixelArray, 0, pixelsToNative, pixelArray.Length);

            IntPtr pixelsFromNative = PhotoFilterNativeMethods.ApplyAntiqueFilter(pixelsToNative, size);
            Marshal.Copy(pixelsFromNative, pixelArray, 0, size);
            Marshal.FreeHGlobal(pixelsToNative);
            Marshal.FreeHGlobal(pixelsFromNative);

            return pixelArray;
        }


    }
}
