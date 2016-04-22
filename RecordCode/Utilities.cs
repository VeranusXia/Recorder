
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
//using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Linq;

namespace RecordCode
{
    public static class Utilities
    {
        public static bool HasNewVersion;
        public static System.Collections.Generic.Dictionary<int, string> IdToChampion;
        public static int Version = 106;
        public static System.Collections.Generic.Dictionary<string, string> LoLObserveServersIpMapping = new System.Collections.Generic.Dictionary<string, string>
		{

            {"HN1_NEW","dg-sg-varnish.lol.qq.com:8088"},
            {"HN2","dg-sg-varnish.lol.qq.com:8088"},
            {"HN3","dg-sg-varnish.lol.qq.com:8088"},
            {"HN4","cd-sg-varnish.lol.qq.com:8088"},
            {"HN5","dg-sg-varnish.lol.qq.com:8088"},
            {"HN6","dg-sg-varnish.lol.qq.com:8088"},
            {"HN7","dg-sg-varnish.lol.qq.com:8088"},
            {"HN8","dg-sg-varnish.lol.qq.com:8088"},
            {"HN9","cd-sg-varnish.lol.qq.com:8088"},
            //{"HN9","nj-sg-varnish.lol.qq.com:8088"},
            {"HN10","dg-sg-varnish.lol.qq.com:8088"},
            {"HN11","dg-sg-varnish.lol.qq.com:8088"},
            {"HN12","cd-sg-varnish.lol.qq.com:8088"},
            {"HN13","dg-sg-varnish.lol.qq.com:8088"},
            {"HN14_NEW","nj-sg-varnish.lol.qq.com:8088"},
            {"HN15","nj-sg-varnish.lol.qq.com:8088"},
            {"HN16","nj-sg-varnish.lol.qq.com:8088"},
            {"HN17","dg.ob.tgp.qq.com:8088"},
            {"HN18_NEW","dg-sg-varnish.lol.qq.com:8088"},
            {"HN19","cd-sg-varnish.lol.qq.com:8088"},
            {"WT1","jn-sg-varnish.lol.qq.com:8088"},
            {"WT2","jn-sg-varnish.lol.qq.com:8088"},
            {"WT3","tj-sg-varnish.lol.qq.com:8088"},
            {"WT4","jn-sg-varnish.lol.qq.com:8088"},
            {"WT5","tj-sg-varnish.lol.qq.com:8088"},
            {"WT6","tj-sg-varnish.lol.qq.com:8088"},

            {"国服线路-东莞","dg.ob.tgp.qq.com:8088"},
            {"国服线路-南京","nj.ob.tgp.qq.com:8088"},
            {"国服线路-杭州","hz.ob.tgp.qq.com:8088"},
            {"国服线路-成都","cd.ob.tgp.qq.com:8088"},
            {"国服线路-济南","jn.ob.tgp.qq.com:8088"},
            {"国服线路-天津","tj.ob.tgp.qq.com:8088"},
            {"KR","110.45.191.11:80"},
            {"NA1","216.133.234.17:8088"},
            {"TEST2","110.45.191.11:80"},
            {"OB1","nj-sg-varnish.lol.qq.com:8088"},
            {"OB2","TJ-sg-varnish.lol.qq.com:8088"},
            {"OB3","dg.ob.tgp.qq.com:8088"},
            {"OB4","HZ-sg-varnish.lol.qq.com:8088"},
            {"OB5","cd-sg-varnish.lol.qq.com:8088"},
            {"OB6","jn-sg-varnish.lol.qq.com:8088"},
 
		};
        public static System.Collections.Generic.List<string> LoLObservePlatformList = new System.Collections.Generic.List<string>
		{
            "OB1",
            "OB2",
            "OB3",
            "OB4",
            "OB5",
            "OB6"
		};
        public static System.Collections.Generic.List<string> LoLObserveServerList = new System.Collections.Generic.List<string>
		{
            //"KR",
            //"NA1",
            "TEST2",
            "国服线路-东莞",
            "国服线路-南京",
            "国服线路-杭州",
            "国服线路-成都",
            "国服线路-济南",
            "国服线路-天津",
		};
        //public static event MainWindow.BackGroundInitDoneDelegate StartupDone;
        public static string GetSimpleVersionNumber(string s)
        {
            string[] array = s.Split(new char[]
			{
				'.'
			});
            return string.Format("{0}.{1}", array[0], array[1]);
        }
        public static int CompareVersion(string v1, string v2)
        {
            string[] array = v1.Split(new char[]
			{
				'.'
			}, 2);
            string[] array2 = v2.Split(new char[]
			{
				'.'
			}, 2);
            if (uint.Parse(array[0]) > uint.Parse(array2[0]))
            {
                return 1;
            }
            if (uint.Parse(array[0]) < uint.Parse(array2[0]))
            {
                return -1;
            }
            int num = v1.IndexOf('.');
            int num2 = v2.IndexOf('.');
            if (num == 0 || num2 == 0)
            {
                return 0;
            }
            return Utilities.CompareVersion(v1.Substring(num + 1), v2.Substring(num2 + 1));
        }
        public static void GetNAServerInfo()
        {
            //Utilities.HasNewVersion = Utilities.IsUpdateAvailable();
            Utilities.GetChampionNumber();
            Utilities.ReadChampionTxt();
            //Utilities.CheckNAServerVersion();
            //if (Utilities.StartupDone != null)
            //{
            //    Utilities.StartupDone();
            //}
        }
        public static string CheckNAServerVersion()
        { 
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string input = webClient.DownloadString("http://gameinfo.na.leagueoflegends.com/assets/js/na.js");
                    Regex regex = new Regex("\"v\":\"(?<V>[\\.0-9]+)\",", RegexOptions.IgnoreCase);
                    Match match = regex.Match(input);
                    return match.Groups["V"].Value;
                }
            }
            catch (System.Exception)
            {
                return "";
            }
            //if (string.Compare(Properties.Settings.Default.NAVersion, value) != 0)
            //{
            //    Properties.Settings.Default.NAVersion = value;
            //}
        }


        public static int GetChampionidByName(string name)
        {
            if (IdToChampion.Where(u => u.Value == name) != null)
            {
                return IdToChampion.FirstOrDefault(u => u.Value == name).Key;
            }
            else
                return 0;
        }


        public static void ReadChampionTxt()
        {
            Utilities.IdToChampion = new System.Collections.Generic.Dictionary<int, string>();



            if (!System.IO.File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\LOLObserver Replay\\" + "Champions\\Champions.txt"))

            // if (!System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Champions\\Champions.txt"))
            {
                return;
            }
            char[] separator = new char[]
            {
                ','
            };
            System.IO.StreamReader streamReader = new System.IO.StreamReader(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\LOLObserver Replay\\" + "Champions\\Champions.txt");
            while (!streamReader.EndOfStream)
            {
                string[] array = streamReader.ReadLine().Split(separator);
                Utilities.IdToChampion.Add(int.Parse(array[0]), array[1]);
            }
            streamReader.Close();
        }
        public static void GetChampionNumber()
        {
            MatchCollection matchCollection = null;
            WebClient webClient = new WebClient();
            
            try
            {
                string input = webClient.DownloadString(string.Format("http://ddragon.leagueoflegends.com/cdn/{0}/data/en_US/champion.jsonp", CheckNAServerVersion()));
                Regex regex = new Regex("\"id\":\"(?<NAME>[a-zA-z]+)\",\"key\":\"(?<ID>[0-9]+)\",", RegexOptions.IgnoreCase);
                matchCollection = regex.Matches(input);
            }
            catch
            {
                return;
            }
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\LOLObserver Replay\\" + "Champions\\Champions.txt", false);
            if (Utilities.IdToChampion == null)
                Utilities.IdToChampion = new Dictionary<int, string>();
            
            foreach (Match match in matchCollection)
            {
                int num = int.Parse(match.Groups["ID"].Value);
                if (!Utilities.IdToChampion.ContainsKey(num))
                {
                    Utilities.IdToChampion.Add(num, match.Groups["NAME"].Value);
                }
                streamWriter.WriteLine(string.Format("{0},{1}", num, match.Groups["NAME"].Value));
            }
            streamWriter.Close();
        }
        public static Process GetProcess(string pName)
        {
            Process[] processesByName = Process.GetProcessesByName(pName);
            if (processesByName.Length == 0)
            {
                return null;
            }
            return processesByName[0];
        }
        public static int GetProcessCount(string pName)
        {
            Process[] processesByName = Process.GetProcessesByName(pName);
            return processesByName.Length;
        }
        public static void UnhandledExceptonHandler(object sender, System.UnhandledExceptionEventArgs args)
        {
            System.Exception ex = (System.Exception)args.ExceptionObject;
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
            //if (MessageBox.Show(string.Format(Utilities.GetString("AskSendReport"), text), Utilities.GetString("BR"), MessageBoxButton.OKCancel) != MessageBoxResult.Cancel)
            //{
            //    CrashReporter crashReporter = new CrashReporter();
            //    crashReporter.AddFile(text);
            //    if (crashReporter.SendToServer())
            //    {
            //        MessageBox.Show(Utilities.GetString("SendSuccess"));
            //        return;
            //    }
            //    MessageBox.Show(Utilities.GetString("SendFailed"));
            //}
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
            Application.Exit();
            //if (MessageBox.Show(string.Format(Utilities.GetString("AskSendReport"), text), Utilities.GetString("BR"), MessageBoxButton.OKCancel) != MessageBoxResult.Cancel)
            //{
            //    CrashReporter crashReporter = new CrashReporter();
            //    crashReporter.AddFile(text);
            //    if (crashReporter.SendToServer())
            //    {
            //        MessageBox.Show(Utilities.GetString("SendSuccess"));
            //        return;
            //    }
            //    MessageBox.Show(Utilities.GetString("SendFailed"));
            //}
        }
        //public static bool IsUpdateAvailable()
        //{
        //    int num = 0;
        //    using (WebClient webClient = new WebClient())
        //    {
        //        try
        //        {
        //            //num = int.Parse(webClient.DownloadString("http://ahri.tw/BaronReplays/Version.php"));
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    return Utilities.Version < num;
        //}
        public static string SearchFileInAncestor(string fullPath, string fName)
        {
            string[] array = fullPath.Split(new char[]
			{
				'\\'
			});
            for (int i = array.Length - 1; i >= 0; i--)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                for (int j = 0; j < i; j++)
                {
                    stringBuilder.Append(array[j]);
                    stringBuilder.Append('\\');
                }
                if (System.IO.File.Exists(stringBuilder.ToString() + fName))
                {
                    return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
                }
            }
            return null;
        }
        public static bool SelectLoLExePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "League of Legends.exe|League of Legends.exe";
            // openFileDialog.Title = Utilities.GetString("PickLoLExe");
            openFileDialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyComputer);
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == string.Empty)
            {
                return false;
            }
            //Properties.Settings.Default.LoLGameExe = openFileDialog.FileName;
            //Properties.Settings.Default.Save();
            return true;
        }
        public static void IfDirectoryNotExitThenCreate(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
        //public static BitmapImage GetBitmapImage(string s)
        //{
        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        //    using (System.IO.FileStream fileStream = new System.IO.FileStream(s, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //    {
        //        memoryStream.SetLength(fileStream.Length);
        //        fileStream.Read(memoryStream.GetBuffer(), 0, (int)fileStream.Length);
        //        memoryStream.Flush();
        //        fileStream.Close();
        //    }
        //    BitmapImage bitmapImage = new BitmapImage();
        //    try
        //    {
        //        bitmapImage.BeginInit();
        //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapImage.StreamSource = memoryStream;
        //        bitmapImage.EndInit();
        //        bitmapImage.Freeze();
        //    }
        //    catch
        //    {
        //        throw new System.Exception(string.Format("Error loading file: {0}", s));
        //    }
        //    return bitmapImage;
        //}
        //public static string GetString(string name)
        //{
        //    string result;
        //    try
        //    {
        //        //switch (name.ToLower())
        //        //{

        //        //    case "br":
        //        //        return "LOLObserver Replay";
        //        //    case "remaingamestatus":
        //        //        return "剩余{0}场比赛录制中";
        //        //    case "alreadyplaying":
        //        //        return "程序已经在运行中了";
        //        //    case "autostart":
        //        //        return "游戏开始后自动录像";
        //        //    default:
        //        result = (Application.Current.FindResource(name) as string);
        //        //}
        //    }
        //    catch
        //    {
        //        return name;
        //    }
        //    return result;
        //}
    }
}
