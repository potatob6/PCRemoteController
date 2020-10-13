using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuckKBTServer
{
    static class ProgramPhone
    {
        public static int count = 1;
        public static Dictionary<string, DialogToPhoneClient> phoneClients = new Dictionary<string, DialogToPhoneClient>();
        private static object obj = new object();

        public static void syncAdd(string n,Socket s1,string flag)
        {
            
            lock (obj)
            {
                bool exist = checkNameExist(n);
                if (exist)
                {
                    //存在
                    DialogToPhoneClient dtpc;
                    var ex = phoneClients.TryGetValue(n, out dtpc);
                    dtpc.selfIndex = n;
                    if (flag.Equals("4416")) dtpc.s4416 = s1;
                    else if (flag.Equals("4417")) dtpc.s4417 = s1;
                    else dtpc.s4418 = s1;

                }
                else
                {
                    //不存在则创建
                    DialogToPhoneClient dpc = new DialogToPhoneClient();
                    dpc.selfIndex = n;
                    if (flag.Equals("4416")) dpc.s4416 = s1;
                    else if (flag.Equals("4417")) dpc.s4417 = s1;
                    else dpc.s4418 = s1;
                    dpc.start();
                    //加入队列
                    phoneClients.Add(n, dpc);
                }
            }
        }

        public static void startListenPhoneClients()
        {
            //开启3个端口监听
            Thread th4416 = new Thread(new ThreadStart(lis4416));
            th4416.Start();
            DateTime now = DateTime.Now;
            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"--4416端口开启--", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            Thread th4417 = new Thread(new ThreadStart(lis4417));
            th4417.Start();
            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"--4417端口开启--", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            Thread th4418 = new Thread(new ThreadStart(lis4418));
            th4418.Start();
            Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"--4418端口开启--", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        public static bool checkNameExist(string name)
        {
            lock (obj)
            {
                DialogToPhoneClient dpc;
                bool exist = phoneClients.TryGetValue(name, out dpc);
                return exist;
            }
            
        }

        /// <summary>
        /// 服务器接收文字端
        /// </summary>
        public static void lis4416()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 4416);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(13);
            while (true)
            {
                Socket s1 = s.Accept();
                byte[] bytes = new byte[128];
                int len = s1.Receive(bytes);
                string n = Encoding.UTF8.GetString(bytes, 0, len);
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"一个4416端口已连接,名字为:" + n, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                syncAdd(n, s1, "4416");
            }
        }

        public static void lis4417()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 4417);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(13);
            while (true)
            {
                Socket s1 = s.Accept();
                byte[] bytes = new byte[128];
                int len = s1.Receive(bytes);
                string n = Encoding.UTF8.GetString(bytes, 0, len);
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"一个4417端口已连接,名字为:" + n, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                syncAdd(n, s1, "4417");
            }
        }

        public static void lis4418()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 4418);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(13);
            while (true)
            {
                Socket s1 = s.Accept();
                byte[] bytes = new byte[128];
                int len;
                len = s1.Receive(bytes);
                string n = Encoding.UTF8.GetString(bytes, 0, len);
                DateTime now = DateTime.Now;
                Console.WriteLine("[{0}/{1}/{2} {3}:{4}:{5}] "+"一个4418端口已连接,名字为:" + n, now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                syncAdd(n, s1, "4418");
            }
        }
    }
}
