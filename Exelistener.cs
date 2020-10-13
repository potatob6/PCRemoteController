using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NvidiaStuUpdater
{
    class Exelistener
    {
        private bool periodRun = false;//先前是否已经开启
        public string exeName = string.Empty;
        public bool stop = false;
        public DialogToServer dts;
        /// <summary>
        /// 主要是针对某一程序的开启和关闭
        /// </summary>
        public void startListen(string exe_n)
        {
            try
            {
                this.exeName = exe_n;
                Thread thread = new Thread(new ThreadStart(Listen));
                thread.Start();
            }
            catch (Exception)
            {
                Process.GetCurrentProcess().Kill();
            }
            
            return;
        }

        public void changeEXE(string name)
        {
            this.periodRun = false;
            this.exeName = name;
        }
        /// <summary>
        /// 子线程执行函数
        /// </summary>
        public void Listen()
        {
            while (!stop)
            {
                Process[] ps = Process.GetProcessesByName(exeName);
                if (ps.Length >= 1)
                {
                    if (!periodRun)
                    {
                        periodRun = true;
                        dts.sendMsgToSever("侦听到开启" + exeName);
                    }
                }
                else
                {
                    if (periodRun)
                    {
                        periodRun = false;
                        dts.sendMsgToSever("侦听到关闭" + exeName);
                    }
                    
                }
                Thread.Sleep(500);
            }
        }
    }
}
