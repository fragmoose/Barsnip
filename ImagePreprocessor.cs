using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace Barsnip
{
    public class ImagePreprocessor
    {
        public Mat PreprocessImage(Mat inputImage, int denoiseStrength = 10, bool sharpen = true, bool autoRotate = true)
        {
            // Convert to grayscale
            Mat grayImage = ConvertToGrayscale(inputImage);

            // Denoise the image
            Mat denoisedImage = DenoiseImage(grayImage, denoiseStrength);

            // Sharpen if requested
            Mat sharpenedImage = sharpen ? SharpenImage(denoisedImage) : denoisedImage;

            // Deskew the image
            Mat deskewedImage = DeskewImage(sharpenedImage);

            // Auto-rotate if requested
            Mat finalImage = autoRotate ? AutoRotateImage(deskewedImage) : deskewedImage;

            // Clean up intermediate images
            grayImage.Dispose();
            denoisedImage.Dispose();
            if (sharpenedImage != denoisedImage) sharpenedImage.Dispose();
            if (deskewedImage != finalImage) deskewedImage.Dispose();

            return finalImage;
        }

        private Mat ConvertToGrayscale(Mat inputImage)
        {
            Mat grayImage = new Mat();
            if (inputImage.NumberOfChannels == 1)
            {
                inputImage.CopyTo(grayImage);
            }
            else
            {
                CvInvoke.CvtColor(inputImage, grayImage, ColorConversion.Bgr2Gray);
            }
            return grayImage;
        }

        private Mat DenoiseImage(Mat grayImage, int strength)
        {
            Mat denoisedImage = new Mat();
            strength = Math.Max(0, Math.Min(30, strength));

            CvInvoke.FastNlMeansDenoising(grayImage, denoisedImage,
                h: strength,
                templateWindowSize: 7,
                searchWindowSize: 21);

            return denoisedImage;
        }

        private Mat SharpenImage(Mat inputImage)
        {
            Mat sharpenedImage = new Mat();
            Mat blurred = new Mat();
            CvInvoke.GaussianBlur(inputImage, blurred, new Size(0, 0), 3);
            CvInvoke.AddWeighted(inputImage, 1.5, blurred, -0.5, 0, sharpenedImage);
            blurred.Dispose();
            return sharpenedImage;
        }

        private Mat DeskewImage(Mat grayImage)
        {
            Mat binary = new Mat();
            CvInvoke.Threshold(grayImage, binary, 0, 255, ThresholdType.Otsu | ThresholdType.Binary);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            double maxAngle = 0;
            double maxArea = 0;

            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    RotatedRect rect = CvInvoke.MinAreaRect(contours[i]);
                    maxAngle = rect.Angle;

                    if (rect.Size.Width < rect.Size.Height)
                    {
                        maxAngle += 90;
                    }
                }
            }

            if (Math.Abs(maxAngle) > 1)
            {
                Mat rotated = new Mat();
                using (Mat rotationMatrix = new Mat())
                {
                    PointF center = new PointF(grayImage.Cols / 2f, grayImage.Rows / 2f);
                    CvInvoke.GetRotationMatrix2D(center, maxAngle, 1.0, rotationMatrix);
                    CvInvoke.WarpAffine(grayImage, rotated, rotationMatrix, grayImage.Size,
                        Inter.Linear, Warp.Default, BorderType.Replicate);
                }

                binary.Dispose();
                contours.Dispose();
                hierarchy.Dispose();

                return rotated;
            }

            binary.Dispose();
            contours.Dispose();
            hierarchy.Dispose();

            Mat result = new Mat();
            grayImage.CopyTo(result);
            return result;
        }

        private Mat AutoRotateImage(Mat grayImage)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(grayImage, edges, 50, 150);

            // Updated HoughLinesP call for newer EmguCV versions
            LineSegment2D[] lines = CvInvoke.HoughLinesP(
                edges,
                rho: 1,
                theta: Math.PI / 180,
                threshold: 100,
                minLineLength: grayImage.Cols / 2);
            // maxLineGap parameter removed in newer versions

            if (lines.Length == 0)
            {
                edges.Dispose();
                Mat result = new Mat();
                grayImage.CopyTo(result);
                return result;
            }

            double totalAngle = 0;
            int count = 0;

            foreach (LineSegment2D line in lines)
            {
                double angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180 / Math.PI;
                if (Math.Abs(angle) < 30 || Math.Abs(angle) > 60)
                {
                    totalAngle += angle;
                    count++;
                }
            }

            edges.Dispose();

            if (count == 0)
            {
                Mat result = new Mat();
                grayImage.CopyTo(result);
                return result;
            }

            double avgAngle = totalAngle / count;
            double rotationAngle = 0;

            if (Math.Abs(avgAngle) > 45)
            {
                rotationAngle = (avgAngle > 0) ? 90 : -90;
            }

            if (Math.Abs(rotationAngle) > 1)
            {
                Mat rotated = new Mat();
                using (Mat rotationMatrix = new Mat())
                {
                    PointF center = new PointF(grayImage.Cols / 2f, grayImage.Rows / 2f);
                    CvInvoke.GetRotationMatrix2D(center, rotationAngle, 1.0, rotationMatrix);
                    CvInvoke.WarpAffine(grayImage, rotated, rotationMatrix, grayImage.Size,
                        Inter.Linear, Warp.Default, BorderType.Replicate);
                }
                return rotated;
            }

            Mat noRotateResult = new Mat();
            grayImage.CopyTo(noRotateResult);
            return noRotateResult;
        }
    }
}