using Cognex.InSight;
using Cognex.InSight.Controls.Display;
using GreatechApp.Core.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace GreatechApp.Services.Utilities
{
    public partial class InSightDisplayControl : Form
    {
        public string Ip;
        private readonly IEventAggregator m_Events;
        private DispatcherTimer tmrCheckEnableCheckBox;

        public InSightDisplayControl(string ip, IEventAggregator eventAggregator)
        {
            Ip = ip;
            m_Events = eventAggregator;
            InitializeComponent();
            //FrontLight
            cvsInSightDisplay1.Connect(ip, "admin", "", true);
            cvsInSightDisplay1.SoftOnline = true;
            //Thread.Sleep(500);
            //if (cvsInSightDisplay1.Connected)
            //{
            cvsInSightDisplay1.Edit.LiveAcquire.Activated = false;
            //cvsInSightDisplay1.SoftOnline = false;
            //cvsInSightDisplay1.Edit.LiveAcquire.Activated = true;
            //cvsInSightDisplay1.ShowGrid = true;
            //cvsInSightDisplay1.ShowGraphics = true;
            //cvsInSightDisplay1.ShowImage = true;
            //}
            cvsInSightDisplay1.ImageZoomMode = CvsDisplayZoom.Fit;
            SaveLiveVideo.Enabled = false;

            timer1 = new Timer();
            timer1.Interval = 1000;
            timer1.Tick += tmrCheckEnableCheckBox_Tick;
            timer1.Start();
        }
        public void EnableLive()
        {
            if (cvsInSightDisplay1.SoftOnline)
            {
                cvsInSightDisplay1.SoftOnline = false; //quit from ready to inspect mode 
            }
            else
            {
                cvsInSightDisplay1.SoftOnline = true; //ready to inspect mode
            }
        }
        public void LiveAcquire()
        {
            cvsInSightDisplay1.Edit.LiveAcquire.Activated = true;
            chkLive.Checked = true;
        }

        public void EnableShowImage()
        {
            cvsInSightDisplay1.ShowImage = true;
            chkImage.Checked = true;
        }

        public void Snapshot()
        {
            try
            {
                cvsInSightDisplay1.ShowImage = true;
                string filename = DateTime.Now.ToString("hhmmss-ddMMyyyy") + "Image.JPEG";
                string path = @"..\Snapshot\" + filename;
                cvsInSightDisplay1.SaveBitmap(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private void optNone_CheckedChanged(object sender, EventArgs e)
        {
            if (optNone.Checked)
            {
                cvsInSightDisplay1.ImageZoomMode = CvsDisplayZoom.None;
                //cvsInSightDisplay2.ImageZoomMode = CvsDisplayZoom.None;
            }
        }

        private void optFit_CheckedChanged(object sender, EventArgs e)
        {
            if (optFit.Checked)
            {
                cvsInSightDisplay1.ImageZoomMode = CvsDisplayZoom.Fit;
                //cvsInSightDisplay2.ImageZoomMode = CvsDisplayZoom.Fit;
            }
        }

        private void optFill_CheckedChanged(object sender, EventArgs e)
        {
            if (optNone.Checked)
            {
                cvsInSightDisplay1.ImageZoomMode = CvsDisplayZoom.None;
                //cvsInSightDisplay2.ImageZoomMode = CvsDisplayZoom.None;
            }
        }

        private void chkLive_CheckedChanged(object sender, EventArgs e)
        {
            if (chkLive.Checked)
            {
                cvsInSightDisplay1.Edit.LiveAcquire.Activated = true;
                //cvsInSightDisplay2.Edit.LiveAcquire.Activated = true;
            }
            else
                cvsInSightDisplay1.Edit.LiveAcquire.Activated = false;
            //cvsInSightDisplay2.Edit.LiveAcquire.Activated = false;
        }

        private void InSightDisplayControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            //cvsInSightDisplay1.Edit.LiveAcquire.Activated = false;
            ////cvsInSightDisplay2.Edit.LiveAcquire.Activated = false;
            ////cvsInSightDisplay1.Connect(Ip, "admin", "", true);
            ////cvsInSightDisplay1.Disconnect();

        }

        private void chkGrid_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGrid.Checked)
            {
                cvsInSightDisplay1.ShowGrid = true;
                //cvsInSightDisplay2.ShowGrid = true;
            }
            else
                cvsInSightDisplay1.ShowGrid = false;
            //cvsInSightDisplay2.ShowGrid = false;
        }

        private void chkImage_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkImage.Checked)
                {
                    cvsInSightDisplay1.ShowImage = true;
                    string path = @"C:\Users\hychuah\Desktop\TestImage\Image";
                    //string filename = "Image" + DateTime.Now;
                    cvsInSightDisplay1.SaveBitmap(path);

                }
                else
                    cvsInSightDisplay1.ShowImage = false;
                //cvsInSightDisplay2.ShowImage = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); ;
            }
        }

        private void chkOnline_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOnline.Checked)
            {
                cvsInSightDisplay1.SoftOnline = true;
                //cvsInSightDisplay2.SoftOnline = true;
            }
            else
                cvsInSightDisplay1.SoftOnline = false;
            //cvsInSightDisplay2.SoftOnline = false;
        }

        private void InSightDisplayControl_Load(object sender, EventArgs e)
        {

            cvsInSightDisplay1.SoftOnline = true;
            //if (cvsInSightDisplay1.SoftOnline)
            //{
            //    cvsInSightDisplay1.SoftOnline = false;

            //}
            //else
            //{
            //    cvsInSightDisplay1.SoftOnline = true;
            //}
            ////Thread.Sleep(500);
            //if (!cvsInSightDisplay1.Edit.LiveAcquire.Activated)
            //{

            //    cvsInSightDisplay1.Edit.LiveAcquire.Activated = true;
            //}
            //InSightDisplayControl.StartPosition = System.Windows.Forms.FormStartPosition.Manual;

        }

        private void SaveLiveVideo_CheckedChanged(object sender, EventArgs e)
        {

            if (SaveLiveVideo.Checked)
            {
                m_Events.GetEvent<StartGetLiveVideo>().Publish();
            }

        }
        private void SaveLiveImage_CheckedChanged(object sender, EventArgs e)
        {
            if (SaveLiveImage.Checked)
            {
                m_Events.GetEvent<StartGetLiveImage>().Publish();
            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (SaveLiveImage.Checked)
                {
                    m_Events.GetEvent<GetLiveImage>().Publish();
                }
                if (SaveLiveVideo.Checked)
                {
                    m_Events.GetEvent<GetLiveVideo>().Publish();
                }
                if (!SaveLiveImage.Checked && !SaveLiveVideo.Checked)
                {
                    System.Windows.MessageBox.Show("No Action Selected", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                SaveLiveImage.Checked = false;
                SaveLiveVideo.Checked = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); ;
            }

        }

        private void InSightDisplayControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            cvsInSightDisplay1.Edit.LiveAcquire.Activated = false;
            m_Events.GetEvent<FormCloseConnection>().Publish();
            timer1.Stop();
        }

        private void tmrCheckEnableCheckBox_Tick(object sender, EventArgs e)
        {
            if (cvsInSightDisplay1.Edit.LiveAcquire.Activated)
            {
                SaveLiveVideo.Enabled = true;
            }
            else
            {
                SaveLiveVideo.Enabled = false;
            }
        }
    }


}
