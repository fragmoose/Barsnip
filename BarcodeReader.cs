using System;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;

namespace Barsnip
{
    public class BarcodeReader
    {
        private readonly BarcodeReaderGeneric _reader;

        public BarcodeReader()
        {
            _reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                TryInverted = true,
                Options = new DecodingOptions
                {
                    PossibleFormats = new[]
                    {
                        BarcodeFormat.UPC_A,
                        BarcodeFormat.UPC_E,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.ITF,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.AZTEC,
                        BarcodeFormat.PDF_417,
                        BarcodeFormat.CODABAR
                    },
                    TryHarder = true
                }
            };
        }

        public BarcodeResult ReadBarcode(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null");

            try
            {
                var luminanceSource = new BitmapLuminanceSource(bitmap);
                var result = _reader.Decode(luminanceSource);

                if (result != null)
                {
                    return new BarcodeResult
                    {
                        Success = true,
                        Text = result.Text,
                        Format = result.BarcodeFormat.ToString(),
                        RawBytes = result.RawBytes
                    };
                }

                return new BarcodeResult
                {
                    Success = false,
                    ErrorMessage = "No barcode could be detected in the provided image"
                };
            }
            catch (Exception ex)
            {
                return new BarcodeResult
                {
                    Success = false,
                    ErrorMessage = $"Error reading barcode: {ex.Message}"
                };
            }
        }

        private class BitmapLuminanceSource : BaseLuminanceSource
        {
            public BitmapLuminanceSource(Bitmap bitmap)
                : base(bitmap.Width, bitmap.Height)
            {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                try
                {
                    var stride = Math.Abs(data.Stride);
                    var pixels = new byte[stride * bitmap.Height];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    CalculateLuminance(pixels, bitmap.PixelFormat, stride);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            }

            private BitmapLuminanceSource(byte[] luminances, int width, int height)
                : base(width, height)
            {
                luminances.CopyTo(luminances, 0);
            }

            protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
            {
                return new BitmapLuminanceSource(newLuminances, width, height);
            }

            private void CalculateLuminance(byte[] rgbRawBytes, PixelFormat pixelFormat, int stride)
            {
                switch (pixelFormat)
                {
                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format32bppArgb:
                        for (int index = 0, luminanceIndex = 0; index < rgbRawBytes.Length && luminanceIndex < luminances.Length; index += 4, luminanceIndex++)
                        {
                            // Calculate luminance from RGB components
                            var b = rgbRawBytes[index];
                            var g = rgbRawBytes[index + 1];
                            var r = rgbRawBytes[index + 2];
                            luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                        }
                        break;
                    case PixelFormat.Format24bppRgb:
                        for (int index = 0, luminanceIndex = 0; index < rgbRawBytes.Length && luminanceIndex < luminances.Length; index += 3, luminanceIndex++)
                        {
                            var b = rgbRawBytes[index];
                            var g = rgbRawBytes[index + 1];
                            var r = rgbRawBytes[index + 2];
                            luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                        }
                        break;
                    case PixelFormat.Format8bppIndexed:
                        Array.Copy(rgbRawBytes, luminances, Math.Min(rgbRawBytes.Length, luminances.Length));
                        break;
                    default:
                        throw new NotSupportedException($"Pixel format {pixelFormat} is not supported");
                }
            }

            private const int RChannelWeight = 19562; // 0.299 * 65536
            private const int GChannelWeight = 38550; // 0.587 * 65536
            private const int BChannelWeight = 7424;  // 0.114 * 65536
            private const int ChannelWeight = 16;     // Right shift amount
        }
    }

    public class BarcodeResult
    {
        public bool Success { get; set; }
        public string Text { get; set; }
        public string Format { get; set; }
        public byte[] RawBytes { get; set; }
        public string ErrorMessage { get; set; }
    }
}