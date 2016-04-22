
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions; 
//using System.Windows.Media.Imaging; 
using System.Linq;

namespace Recorder
{
    public static class Utilities
    {  
        
        public static void UnhandledExceptonHandler(object sender, System.UnhandledExceptionEventArgs args)
        {
            System.Exception ex = (System.Exception)args.ExceptionObject;
            string text = System.Environment.CurrentDirectory+ "\\" + System.DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + "_CrashLog.txt";
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(text);
            streamWriter.WriteLine("Start of error log:");
            streamWriter.WriteLine("Operating system: {0}", System.Environment.OSVersion.VersionString);
            streamWriter.WriteLine("64-bit operating system: {0}", System.Environment.Is64BitOperatingSystem);
            streamWriter.WriteLine("Processor count: {0}", System.Environment.ProcessorCount);
            streamWriter.WriteLine("version: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (ex.Message != null)
            {
                streamWriter.WriteLine("Exception message: {0}", ex.Message);
            }
            if (ex.StackTrace != null)
            {
                streamWriter.WriteLine("Stack trace: {0}", ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                streamWriter.WriteLine("Inner exception");
                if (ex.InnerException.Message != null)
                {
                    streamWriter.WriteLine("Inner exception message: {0}", ex.InnerException.Message);
                }
                if (ex.InnerException.StackTrace != null)
                {
                    streamWriter.WriteLine("Inner exception stack trace: {0}", ex.InnerException.StackTrace);
                }
            }
            streamWriter.WriteLine("End of error log");
            streamWriter.Close();
 
        }
        public static void UnhandledThreadExceptonHandler(object sender, System.Threading.ThreadExceptionEventArgs args)
        {
            System.Exception ex = (System.Exception)args.Exception;
            string text = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\LOLObserver Replay\\" + System.DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + "CrashLog.txt";
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(text);
            streamWriter.WriteLine("Start of error log:");
            streamWriter.WriteLine("Operating system: {0}", System.Environment.OSVersion.VersionString);
            streamWriter.WriteLine("64-bit operating system: {0}", System.Environment.Is64BitOperatingSystem);
            streamWriter.WriteLine("Processor count: {0}", System.Environment.ProcessorCount);
            streamWriter.WriteLine("version: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            if (ex.Message != null)
            {
                streamWriter.WriteLine("Exception message: {0}", ex.Message);
            }
            if (ex.StackTrace != null)
            {
                streamWriter.WriteLine("Stack trace: {0}", ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                streamWriter.WriteLine("Inner exception");
                if (ex.InnerException.Message != null)
                {
                    streamWriter.WriteLine("Inner exception message: {0}", ex.InnerException.Message);
                }
                if (ex.InnerException.StackTrace != null)
                {
                    streamWriter.WriteLine("Inner exception stack trace: {0}", ex.InnerException.StackTrace);
                }
            }
            streamWriter.WriteLine("End of error log");
            streamWriter.Close();
 
        }
 
 
        public static void IfDirectoryNotExitThenCreate(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
      
    }
}
