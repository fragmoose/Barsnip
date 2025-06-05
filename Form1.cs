using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Reflection;
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
                try
                {
                    var barcodeReader = new Barsnip.BarcodeReader();
                    var result = barcodeReader.ReadBarcode(bitmap);

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
                    bitmap.Dispose();
                }
            };

            snipper.StartSelection();
        }



    }
}