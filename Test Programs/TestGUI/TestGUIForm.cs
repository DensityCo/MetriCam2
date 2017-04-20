﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetriCam2;
using MetriCam2.Cameras;
using System.IO.Ports;

namespace TestGUI
{
    public partial class TestGUIForm : Form
    {
        private IAsyncResult setBMPResult;
        private IAsyncResult setSecondBMPResult;
        private Camera cam;

        private delegate void SetBmpDelegate(Bitmap b);

        /// <summary>
        /// Standard constructor.
        /// </summary>
        public TestGUIForm()
        {
            InitializeComponent();
            this.FormClosing += TestGUIForm_FormClosing;

            cam = new BluetechnixEth();
            // CLEye-specific set-up code
            BluetechnixEth argos = cam as BluetechnixEth;
            if (null != argos)
            {
                argos.IPAddress = "192.168.1.147";
            }
        }

        private void TestGUIForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundWorkerGetFrames.CancelAsync();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (cam.IsConnected)
            {
                // if we are already connected, just disable the button and cancel the display thread, the actual disconnection takes place in the *_RunWorkerCompleted method.
                buttonConnect.Enabled = false;
                backgroundWorkerGetFrames.CancelAsync();
            }
            else
            {
                // connect the camera and start the display background worker.
                buttonConnect.Enabled = false;
                try
                {


                    cam.Connect();
                    BluetechnixEth argos = cam as BluetechnixEth;
                    if (null != argos)
                    {
                        argos.SnapshotModeEnabled = true;
                        argos.ActivateChannel(ChannelNames.Color);
                        argos.ActivateChannel(ChannelNames.Amplitude);
                        argos.ActivateChannel(ChannelNames.Distance);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Connection error: " + ex.Message);
                    buttonConnect.Enabled = true;
                    return;
                }
                buttonConnect.Text = "Disconnect";
                backgroundWorkerGetFrames.RunWorkerAsync();
                buttonConnect.Enabled = true;
            }
        }

        private void backgroundWorkerGetFrames_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!backgroundWorkerGetFrames.CancellationPending)
            {
                Bitmap bitmap;

                // capture a new frame
                cam.Update();

                // get the current frame
                bitmap = cam.CalcChannel(ChannelNames.Color).ToBitmap();

                // set the picturebox-bitmap in the main thread to avoid concurrency issues (a few helper methods required, easier/nicer solutions welcome).
                this.InvokeSetBmp(bitmap);
            }
        }
        private void InvokeSetBmp(Bitmap bmp)
        {
            if (setBMPResult == null || setBMPResult.IsCompleted)
            {
                setBMPResult = this.BeginInvoke(new SetBmpDelegate(this.SetImage), bmp);
            }
        }
        private void InvokeSetSecondBmp(Bitmap bmp)
        {
            if (setSecondBMPResult == null || setSecondBMPResult.IsCompleted)
            {
                setSecondBMPResult = this.BeginInvoke(new SetBmpDelegate(this.SetSecondImage), bmp);
            }
        }

        private void SetImage(Bitmap bitmap)
        {
            Bitmap oldBitmap = (Bitmap)pictureBoxImageStream.Image;
            pictureBoxImageStream.Image = bitmap;
            if (oldBitmap != null && oldBitmap != bitmap)
                oldBitmap.Dispose();
        }

        private void SetSecondImage(Bitmap bitmap)
        {
            Bitmap oldBitmap = (Bitmap)pictureBoxSecondImage.Image;
            pictureBoxSecondImage.Image = bitmap;
            if (oldBitmap != null && oldBitmap != bitmap)
                oldBitmap.Dispose();
        }

        private void backgroundWorkerGetFrames_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // disconnect camera and re-enable button.
            cam.Disconnect();
            buttonConnect.Text = "Connect";
            buttonConnect.Enabled = true;
        }
    }
}
