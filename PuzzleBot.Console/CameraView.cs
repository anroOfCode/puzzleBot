// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using PuzzleBot.Control;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace PuzzleBot.Console
{
    // Uses WinForms to create a window on the screen to display an OpenCV
    // matrix image.
    public sealed class CameraView : ICameraView
    {
        private Form _displayForm;
        private Thread _messagePump;

        public Form Form { get { return _displayForm; } }

        public CameraView(string title)
        {
            _displayForm = new Form();
            _displayForm.Text = title;
            _messagePump = new Thread(UpdateLoop);
            _messagePump.Start();
            // Cheap trick to wait for the message pump to warm up.
            Thread.Sleep(100);
        }

        private void UpdateLoop()
        {
            Application.Run(_displayForm);
        }

        // Assumes img is Rgb-formatted pixel data.
        public void UpdateImage(Control.OpenCV.Mat img)
        {
            if (_displayForm.InvokeRequired) {
                _displayForm.Invoke(new Action(() => {
                    UpdateImage(img);
                }));
                return;
            }

            var bImg = new Bitmap(img.Columns, img.Rows, PixelFormat.Format24bppRgb);
            var bits = bImg.LockBits(new Rectangle(0, 0, bImg.Width, bImg.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int i = 0;
            unsafe
            {
                var imgData = img.RawData;
                for (int y = 0; y < img.Rows; y++) {
                    for (int x = 0; x < img.Columns; x++) {
                        for (int j = 0; j < 3; j++) {
                            ((byte*)bits.Scan0.ToPointer())[i] = imgData[i];
                            i++;
                        }
                    }
                }
            }
            _displayForm.BackgroundImage = bImg;
            _displayForm.Width = img.Columns;
            _displayForm.Height = img.Rows;
        }
    }
}
