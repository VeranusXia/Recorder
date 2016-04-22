using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RecordCode
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //处理未捕获的异常
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Utilities.UnhandledThreadExceptonHandler);
            //处理非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Utilities.UnhandledExceptonHandler);


            System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(Utilities.UnhandledExceptonHandler);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
