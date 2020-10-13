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

namespace FuckKBTServer
{
    class DialogToClient
    {
        public Socket client;
        public Thread thRecive;
        public int name = 0;
        public string holdToSend = string.Empty;

        public DialogToClient(Socket client)
        {
            thRecive = new Thread(new ThreadStart(loopRecive));
            this.client = client;
            thRecive.Start();
        }

        /// <summary>
        /// send默认为文字
        /// </summary>
        public void send()
        {
            //TODO:这个函数需要更改,不再是传string了,而是传C2PMSG类型
            if (!holdToSend.Equals(string.Empty))
            {
                C2PMsg m = new C2PMsg();
                m.type = (sbyte)(MSGTYPE._STRING_);
                m.strValue = Encoding.UTF8.GetBytes(holdToSend);
                client.Send(SerializeObj(m));
                holdToSend = string.Empty;
            }
        }
        
        /// <summary>
        /// upload命令传输文件
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="fileURL"></param>
        public void send(string cmd,string fileURL)
        {
            //TODO:需要用新线程执行这个函数
            if (fileURL != null)
            {
                C2PMsg c2PMsg = new C2PMsg();
                c2PMsg.type = (sbyte)MSGTYPE._FILE_;
                c2PMsg.strValue = Encoding.UTF8.GetBytes(cmd);
                c2PMsg.imgValue = GetFileData(fileURL);
                client.Send(SerializeObj(c2PMsg));
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"长度为{6}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, c2PMsg.imgValue.Length);
            }
            else
            {
                C2PMsg c2PMsg = new C2PMsg();
                c2PMsg.type = (sbyte)MSGTYPE._FILE_;
                c2PMsg.strValue = Encoding.UTF8.GetBytes(cmd);
                client.Send(SerializeObj(c2PMsg));
            }
            

        }

        public byte[] SerializeObj(object obj)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        public void loopRecive()
        {
            try
            {
                while (true)
                {
                    byte[] bytes = new byte[12 * 1024 * 1024];
                    int len = client.Receive(bytes);
                    Object msg1 = new Object();
                    DeserializeToObj(bytes, out msg1);
                    C2PMsg msg = (C2PMsg)msg1;
                    if (msg.type == (sbyte)MSGTYPE._STRING_)
                    {
                        //文本转换
                        string n = Encoding.UTF8.GetString(msg.strValue);
                        //输出信息
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"Client" + name + " >",now.Year,now.Month,now.Day,now.Hour,now.Minute,now.Second);
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+n, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        //信息发给各客户端
                        foreach(KeyValuePair<string,DialogToPhoneClient> kv in ProgramPhone.phoneClients)
                        {
                            if (kv.Value.listenIndex == name)
                                kv.Value.sendStr(n);
                        }
                    }
                    else if (msg.type == (sbyte)MSGTYPE._IMAGE_)
                    {
                        string time = DateTime.Now.ToString("yyyyMMddhhmmss");
                        MemoryStream ms = new MemoryStream(msg.imgValue);
                        Bitmap bm = (Bitmap)Image.FromStream(ms);
                        //Don't save, asshole
                        //bm.Save(name+"_"+time + ".jpg");
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"Client" +name + "----接收到图像----图像已经发给各移动端", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        //图片发给各客户端
                        foreach(KeyValuePair<string,DialogToPhoneClient> kv in ProgramPhone.phoneClients)
                        {
                            if (kv.Value.listenIndex == name)
                            {
                                
                                
                                kv.Value.sendBitMap(bm);
                                kv.Value.sendStr("接收到图像:" + time+"长度为:"+ms.ToArray().Length);
                            }
                        }
                        ms.Close();
                    }else if (msg.type == (sbyte)MSGTYPE._FILE_)
                    {
                        //TODO:接收客户端传回来的文件类型
                        WriteByteToFile(msg.imgValue, Encoding.UTF8.GetString(msg.strValue).Split('?')[2]);
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"从Client {6}接收到文件,并保存到了{7}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, name, Encoding.UTF8.GetString(msg.strValue).Split('?')[2]);
                    }
                }
            }catch(Exception ex)
            {
                string errorMessage = ex.Message;
                if (errorMessage.Equals("由于连接方在一段时间后没有正确答复或连接的主机没有反应，连接尝试失败。"))
                {
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+errorMessage, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    
                }else if(errorMessage.Equals("远程主机强迫关闭了一个现有的连接。"))
                {
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+ex.Message, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----Client" + name.ToString() + "断开连接----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    Program.DisconnectClient(this.name);
                }
                else
                {
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+ex.Message, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                }
                
            }
            
            
        }

        public static bool WriteByteToFile(byte[] pReadByte, string fileName)
        {
            FileStream pFileStream = null;
            try
            {
                pFileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
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

        public byte[] GetFileData(string fileUrl)
        {
            FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read);
            try
            {
                byte[] buffur = new byte[fs.Length];
                fs.Read(buffur, 0, (int)fs.Length);

                return buffur;
            }
            catch (Exception ex)
            {
                //MessageBoxHelper.ShowPrompt(ex.Message);
                return null;
            }
            finally
            {
                if (fs != null)
                {

                    //关闭资源
                    fs.Close();
                }
            }
        }


        /// <summary>
        /// 序列转图片
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Bitmap bytes2bmp(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            return new Bitmap((Image)new Bitmap(ms));
        }
        /// <summary>
        /// 序列转文本
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string bytes2string(byte[] bytes)
        {
            string n = Encoding.UTF8.GetString(bytes);
            return n;
        }
        /// <summary>
        /// 序列到结构体的变换
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object bytes2stru(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            Console.WriteLine(bytes.Length);
            Console.WriteLine(size);
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }


        public void DeserializeToObj(byte[] bytes,out object obj)
        {
            obj = null;
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(ms);
        }
    }
}
