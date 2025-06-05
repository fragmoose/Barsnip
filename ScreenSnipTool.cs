using System;
using System.Drawing;
using System.Windows.Forms;

namespace Barsnip
{
    public class ScreenSnipTool
    {
        private Rectangle selectionRect;
        private Point selectionStart;
        private bool isSelecting = false;
        private Bitmap? screenBitmap;
        private Form? overlayForm;
        private Rectangle virtualScreenBounds;

        public event Action<Bitmap>? SnipCompleted;

        public void StartSelection()
        {
            virtualScreenBounds = GetTotalScreenBounds();

            overlayForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                WindowState = FormWindowState.Normal, // Not maximized
                TopMost = true,
                ShowInTaskbar = false,
                Cursor = Cursors.Cross,
                BackColor = Color.Black,
                Opacity = 0.3,
                StartPosition = FormStartPosition.Manual,
                Bounds = virtualScreenBounds,
                Location = virtualScreenBounds.Location
            };

            overlayForm.MouseDown += OverlayForm_MouseDown!;
            overlayForm.MouseMove += OverlayForm_MouseMove!;
            overlayForm.MouseUp += OverlayForm_MouseUp!;
            overlayForm.KeyDown += OverlayForm_KeyDown!;
            overlayForm.Paint += OverlayForm_Paint!;

            // Capture each screen individually first
            screenBitmap = CaptureAllScreens();

            overlayForm.Show();
        }

        private Bitmap CaptureAllScreens()
        {
            Bitmap fullImage = new Bitmap(virtualScreenBounds.Width, virtualScreenBounds.Height);

            using (Graphics g = Graphics.FromImage(fullImage))
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    using (Bitmap screenBmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height))
                    {
                        using (Graphics screenG = Graphics.FromImage(screenBmp))
                        {
                            screenG.CopyFromScreen(screen.Bounds.Location, Point.Empty, screen.Bounds.Size);
                        }

                        // Draw each screen at its correct virtual position
                        g.DrawImage(
                            screenBmp,
                            screen.Bounds.X - virtualScreenBounds.X,
                            screen.Bounds.Y - virtualScreenBounds.Y,
                            screen.Bounds.Width,
                            screen.Bounds.Height);
                    }
                }
            }

            return fullImage;
        }

        private Rectangle GetTotalScreenBounds()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (Screen screen in Screen.AllScreens)
            {
                minX = Math.Min(minX, screen.Bounds.X);
                minY = Math.Min(minY, screen.Bounds.Y);
                maxX = Math.Max(maxX, screen.Bounds.Right);
                maxY = Math.Max(maxY, screen.Bounds.Bottom);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void OverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSelecting = true;
                selectionStart = e.Location;
                selectionRect = new Rectangle(e.Location, Size.Empty);
            }
        }

        private void OverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                selectionRect = new Rectangle(
                    Math.Min(e.X, selectionStart.X),
                    Math.Min(e.Y, selectionStart.Y),
                    Math.Abs(e.X - selectionStart.X),
                    Math.Abs(e.Y - selectionStart.Y));

                overlayForm!.Invalidate();
            }
        }

        private void OverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isSelecting && screenBitmap != null)
            {
                isSelecting = false;

                if (selectionRect.Width > 10 && selectionRect.Height > 10)
                {
                    // Ensure the selection is within bounds
                    selectionRect.Intersect(new Rectangle(0, 0, screenBitmap.Width, screenBitmap.Height));

                    if (selectionRect.Width > 0 && selectionRect.Height > 0)
                    {
                        Bitmap snippedImage = screenBitmap.Clone(selectionRect, screenBitmap.PixelFormat);
                        SnipCompleted?.Invoke(snippedImage);
                    }
                }

                Cleanup();
            }
        }

        private void OverlayForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                isSelecting = false;
                Cleanup();
            }
        }

        private void Cleanup()
        {
            overlayForm?.Close();
            overlayForm?.Dispose();
            screenBitmap?.Dispose();
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            if (isSelecting)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }

                using (Brush brush = new SolidBrush(Color.FromArgb(50, Color.White)))
                {
                    e.Graphics.FillRectangle(brush, selectionRect);
                }
            }
        }
    }
}