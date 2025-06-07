using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace Barsnip
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            verLabel.Text = $"Barsnip v{Assembly.GetExecutingAssembly().GetName().Version}";
        }


        private void snipReadCopy_Click(object sender, EventArgs e)
        {
            var snipper = new Barsnip.ScreenSnipTool();

            snipper.SnipCompleted += (bitmap) =>
            {
                Mat inputImage = null;
                Mat processedImage = null;
                Bitmap processedBitmap = null;

                try
                {
                    // Convert Bitmap to Mat using universal method
                    inputImage = ConvertBitmapToMat(bitmap);

                    // Preprocess the image
                    var preprocessor = new ImagePreprocessor();
                    processedImage = preprocessor.PreprocessImage(
                        inputImage,
                        denoiseStrength: 15,
                        sharpen: true,
                        autoRotate: true);

                    // Convert processed Mat back to Bitmap
                    processedBitmap = ConvertMatToBitmap(processedImage);

                    // Read barcode
                    var barcodeReader = new Barsnip.BarcodeReader();
                    var result = barcodeReader.ReadBarcode(processedBitmap);

                    if (result.Success)
                    {
                        Clipboard.SetText(result.Text);
                        SystemSounds.Beep.Play();
                        label1.Text = "Barcode data copied";
                    }
                    else
                    {
                        label1.Text = "Couldn't read barcode";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing barcode: {ex.Message}",
                                  "Error",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
                finally
                {
                    inputImage?.Dispose();
                    processedImage?.Dispose();
                    processedBitmap?.Dispose();
                    bitmap.Dispose();
                }
            };

            snipper.StartSelection();
        }

        // Universal Bitmap to Mat conversion that works with all versions
        // Corrected Bitmap to Mat conversion
        private Mat ConvertBitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            // Convert to 32bpp ARGB if needed
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                var newBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(newBitmap))
                {
                    g.DrawImage(bitmap, 0, 0);
                }
                bitmap = newBitmap;
            }

            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            try
            {
                Mat mat = new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4);
                CvInvoke.CvtColor(new Mat(bitmap.Height, bitmap.Width, DepthType.Cv8U, 4,
                    bmpData.Scan0, bmpData.Stride), mat, ColorConversion.Bgra2Bgr);
                return mat;
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
        }

        // Reliable Mat to Bitmap conversion
        private Bitmap ConvertMatToBitmap(Mat mat)
        {
            if (mat == null || mat.IsEmpty)
                throw new ArgumentException("Invalid Mat image");

            using (VectorOfByte vb = new VectorOfByte())
            {
                CvInvoke.Imencode(".png", mat, vb);
                using (MemoryStream ms = new MemoryStream(vb.ToArray()))
                {
                    return new Bitmap(ms);
                }
            }
        }
    }
}
