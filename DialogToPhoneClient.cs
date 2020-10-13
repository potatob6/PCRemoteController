using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuckKBTServer
{
    class DialogToPhoneClient
    {
        public int listenIndex = 1;
        public string selfIndex = string.Empty;
        public Socket s4416;  //字符输入
        public Socket s4417;  //字符输出
        public Socket s4418;  //图像输出
        public void start()
        {
            //启用一个线程获取输入
            Thread th = new Thread(new ThreadStart(rec));
            th.Start();
        }

        public void rec()
        {
            while (true)
            {
                byte[] bytes = new byte[128];
                int len = 0;
                if (s4416 == null)
                {
                    Thread.Sleep(100);
                    continue;
                }
                try
                {
                    len = s4416.Receive(bytes);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+selfIndex + " 移动端已断开连接-----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    try
                    {
                        s4418.Close();
                    }
                    catch (Exception) { }

                    try
                    {
                        s4417.Close();
                    }
                    catch (Exception) { }
                    try
                    {
                        s4416.Close();
                    }
                    catch (Exception) { }
                    
                    
                    ProgramPhone.phoneClients.Remove(selfIndex);
                    break;
                }
                
                string n = Encoding.UTF8.GetString(bytes, 0, len);
                if (!n.Equals("Pulse"))
                {
                    string[] n1 = regroup(n.Split('?'));
                    if (n.Length != 0)
                    {
                        listenIndex = int.Parse(n1[0]);
                        DateTime now = DateTime.Now;
                        Console.WriteLine("["+now.Year+"/"+ now.Month + "/" + now.Day +" "+ now.Hour + ":" + now.Minute + ":" + now.Second+"] " + "移动端 " + selfIndex + " 消息:" + n);
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"移动端 " + selfIndex + " 的监听对象是: " + listenIndex, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"移动端队列数:" + ProgramPhone.phoneClients.Count, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        //发送给服务端
                        Program.sendToDialogs(n);
                    }
                    else
                    {
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+selfIndex + " 移动端已断开连接-----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        try
                        {
                            s4418.Close();
                        }
                        catch (Exception) { }

                        try
                        {
                            s4417.Close();
                        }
                        catch (Exception) { }
                        try
                        {
                            s4416.Close();
                        }
                        catch (Exception) { }


                        ProgramPhone.phoneClients.Remove(selfIndex);
                        break;
                    }
                }
                
                
            }
        }

        private string[] regroup(string[] shit)
        {
            string n = "";
            for(int i = 1; i < shit.Length; i++)
            {
                n += shit[i];
            }

            return new string[] { shit[0], n };
        }

        public void sendBitMap(Bitmap bmp)
        {
            string n = ImgToBase64String(bmp);
            DateTime now = DateTime.Now;
            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发送到了一个名字为 " + selfIndex + " 的手机客户端,长度为:" + n.Length, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            s4418.Send(Encoding.UTF8.GetBytes(n+"\r\n"));
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// 图片压缩(降低质量以减小文件的大小)
        /// </summary>
        /// <param name="srcBitmap">传入的Bitmap对象</param>
        /// <param name="destStream">压缩后的Stream对象</param>
        /// <param name="level">压缩等级，0到100，0 最差质量，100 最佳</param>
        public static void Compress(Bitmap srcBitmap, Stream destStream, long level)
        {
            ImageCodecInfo myImageCodecInfo;
            System.Drawing.Imaging.Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;

            // Get an ImageCodecInfo object that represents the JPEG codec.
            myImageCodecInfo = GetEncoderInfo("image/jpeg");

            // Create an Encoder object based on the GUID

            // for the Quality parameter category.
            myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one

            // EncoderParameter object in the array.
            myEncoderParameters = new EncoderParameters(1);

            // Save the bitmap as a JPEG file with 给定的 quality level
            myEncoderParameter = new EncoderParameter(myEncoder, level);
            myEncoderParameters.Param[0] = myEncoderParameter;
            srcBitmap.Save(destStream, myImageCodecInfo, myEncoderParameters);
        }

        public string ImgToBase64String(Bitmap bmp)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                Compress(bmp, ms, Program.comPressLevel);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void sendStr(string n)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(n);
                s4417.Send(bytes,bytes.Length,0);
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发送字符串到 " + selfIndex + " 成功", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            }catch(Exception e)
            {
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发送字符串到 " + selfIndex + " 失败:"+e.Message, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            }
        }
    }
}
