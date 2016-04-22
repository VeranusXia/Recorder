using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RecordCode
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Dictionary<LoLRecorder, Thread> Recorders;
        private lolreplayEntities db = new lolreplayEntities();


        private List<GameInfo> Games { get; set; }
        private List<string> status { get; set; }
        //private List<string> PL { get; set; }
        private bool work { get; set; }
        public void StartRecord()
        {
            Recorders = new Dictionary<LoLRecorder, Thread>();
            Games = new List<GameInfo>();
            status = new List<string>();
            Utilities.GetNAServerInfo();
            work = true;
            Thread fgt = new Thread(FeaturedGameThread);
            fgt.IsBackground = true;
            fgt.Start();
            Thread opgg = new Thread(GetOPGGGames);
            opgg.IsBackground = true;
            opgg.Start();
            Thread rt = new Thread(RecordThread);
            rt.IsBackground = true;
            rt.Start();
            Thread gct = new Thread(ClearMemory);
            gct.IsBackground = true;
            gct.Start();
            Thread insertsql = new Thread(insertSQL);
            insertsql.IsBackground = true;
            insertsql.Start();
        }
        public void StopRecord()
        {
            work = false;
        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        public static void ClearMemory()
        {
            while (true)
            {
                Thread.Sleep(1000000);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
        }

        private void FeaturedGameThread()
        {
            while (work)
            {
                try
                {
                    //PL = db.Player.Where(u => u.DWServerName == "1" || u.DWServerName == "2").Select(u => u.Name).Distinct().ToList();
                    foreach (var item in Utilities.LoLObserveServerList)
                    {
                        //log(DateTime.Now.ToString() + ":" + "Load Feature Game - " + item);
                        GetFeaturedGames(Utilities.LoLObserveServersIpMapping[item]);
                        //log(DateTime.Now.ToString() + ":" + "Load Feature Game - " + item + "(over)");
                    }
                }
                catch { }
                Thread.Sleep(300000);

            }
        }
        private void RecordThread()
        {
            while (work)
            {
                try
                {
                    if (Games.Count > 0)
                    {
                        GameInfo g = GetGameList();
                        Thread t = new Thread(() => StartNewRecoding(g));
                        t.IsBackground = true;
                        t.Start();
                    }
                    else
                        Thread.Sleep(30000);
                    if (LoLRecorder.Recorders.Count > 100)
                    {
                        Thread.Sleep(20000);
                    }
                }
                catch { }
            }
        }


        public GameInfo GetGameList()
        {
            GameInfo g = Games.FirstOrDefault();

            lock (obj2)
            {
                Games.Remove(g);
            }
            return g;

        }
        private void GetOPGGGames()
        {
            while (work)
            {
                try
                {
                    SearchHF();
                }
                catch { }
                Thread.Sleep(300000);

            }

        }

        protected void SearchHF()
        {
            try
            {
                string str = "";

                WebClient _httpClient = new WebClient();
                str = _httpClient.DownloadString("http://op.gg/spectate/pro/");
                //if (!isPro)
                //{
                //    str = this.Replace("<ul class=\"SpectatorSummonerList\">", "</ul>", str, false);
                //}
                //str = this.GetValue("<ul class=\"SpectatorSummonerList\">", "</ul>", str);

                while (str.Contains("<a href=\"/match/observer/id"))
                {
                    string href = this.GetValue("<a href=\"/match/observer/id", "\"", str);
                    str = str.Replace(href, "");
                    string rstr = _httpClient.DownloadString("http://op.gg" + href.Replace("<a href=\"", ""));

                    Regex regex = new Regex("spectator (?<ADDR>[A-Za-z0-9\\.]+:*[0-9]*) (?<KEY>.{32}) (?<GID>[0-9]+) (?<PF>[A-Z0-9_]+)", RegexOptions.IgnoreCase);
                    Match match = regex.Match(rstr);
                    if (match.Success)
                    {
                        lock (obj2)
                        {
                            if (this.Games.FirstOrDefault(u => u.GameId == ulong.Parse(match.Groups["GID"].Value)) == null)
                                this.Games.Add(new GameInfo() { GameId = ulong.Parse(match.Groups["GID"].Value), ObKey = match.Groups["KEY"].Value, PlatformId = match.Groups["PF"].Value, ServerAddress = match.Groups["ADDR"].Value });
                        }
                    }

                }
            }
            catch (Exception exception)
            {

            }
        }

        public string Replace(string str, string value, bool repeat)
        {
            do
            {
                string str2 = this.GetValue(str, value);
                if (!string.IsNullOrEmpty(str2))
                {
                    value = value.Replace(str2, "");
                }
                else
                {
                    repeat = false;
                }
            }
            while (repeat);
            return value;
        }

        public string Replace(string b, string e, string value, bool repeat)
        {
            do
            {
                string str = this.GetValue(b, e, value);
                if (!string.IsNullOrEmpty(str))
                {
                    value = value.Replace(str, "");
                }
                else
                {
                    repeat = false;
                }
            }
            while (repeat);
            return value;
        }
        public string GetValue(string str, string value)
        {
            string str2 = "<" + str;
            string str3 = "</" + str + ">";
            if (value.Contains(str2) && value.Contains(str3))
            {
                int index = value.IndexOf(str2);
                int num2 = (value.Substring(index).IndexOf(str3) + index) + (str.Length + 3);
                value = value.Substring(index, num2 - index);
                return value;
            }
            return "";
        }
        public string GetValue(string b, string e, string value)
        {
            if (value.Contains(b) && value.Contains(e))
            {
                int index = value.IndexOf(b);
                int num2 = ((value.Substring(index + b.Length).IndexOf(e) + index) + e.Length) + b.Length;
                value = value.Substring(index, num2 - index);
                return value;
            }
            return "";
        }

        private void GetFeaturedGames(string address)
        {
            try
            {
                WebClient _httpClient = new WebClient();
                string uriString = string.Format("http://{0}/observer-mode/rest/featured", address);
                _httpClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.FeaturedGameData_DownloadDataCompleted);
                _httpClient.DownloadDataAsync(new Uri(uriString));

            }
            catch (System.Exception)
            {
            }
        }
        private object obj2 = new object();
        private void FeaturedGameData_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {

                if (e.Cancelled)
                {
                    return;
                }
                byte[] result = e.Result;
                string @string = System.Text.Encoding.UTF8.GetString(result);
                JObject jObject = JsonConvert.DeserializeObject<JObject>(@string);
                var gamearray = JsonConvert.DeserializeObject<FeaturedGameJson[]>(jObject["gameList"].ToString()).Where(u => !u.GameMode.Contains("ODIN"));
                //if (checkBox1.Checked)
                //{
                //    gamearray = gamearray.Where(u => u.platformId == "KR" || u.platformId == "HN1_NEW");
                //}
                //else
                {
                    gamearray = gamearray.Where(u => u.platformId == "HN1_NEW" || u.platformId == "NA1" || u.platformId == "TEST2");
                }
                foreach (var item in gamearray.ToArray())
                {
                    //foreach (var pf in Utilities.LoLObservePlatformList)
                    {
                        //string pf = "OB3";
                        string sad = Utilities.LoLObserveServersIpMapping[item.platformId];
                        lock (obj2)
                        {
                            if (this.Games.FirstOrDefault(u => u.GameId == item.gameId && u.PlatformId == item.platformId && u.ServerAddress == sad) == null)
                            {
                                //if (item.participants.Where(u => this.PL.Contains(u.summonerName)).Count() > 0)
                                {
                                    this.Games.Add(new GameInfo() { GameId = item.gameId, ObKey = item.observers["encryptionKey"].ToString(), PlatformId = item.platformId, ServerAddress = sad });

                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void StartNewRecoding(GameInfo g)
        {
            LoLRecorder r = new LoLRecorder(g);

            //log(DateTime.Now.ToString() + ":" + "Start Record Game - GameID:" + g.GameId.ToString() + " & Platform:" + g.PlatformId);
            r.doneEvent += this.RecordDone;
            try
            {
                System.Threading.ThreadStart start = new System.Threading.ThreadStart(r.startRecording);
                System.Threading.Thread thread = new System.Threading.Thread(start);
                lock (obj)
                {
                    this.Recorders.Add(r, thread);
                    thread.IsBackground = true;
                    thread.Start();
                }
            }
            catch { }
            //log(DateTime.Now.ToString() + ":start record -" + g.GameId.ToString() + "-" + g.PlatformId);
        }

        public delegate void RecordDoneDelegate(LoLRecorder sender, bool isSuccess, string reason);
        public void RecordDone(LoLRecorder sender, bool isSuccess, string reason)
        {
            try
            {
                if (this.Recorders.ContainsKey(sender))
                {
                    this.Recorders.Remove(sender);
                }
                if (isSuccess)
                {
                    string ReplayDir = "d:\\replay";
                    string str = sender.record.gameId + ".lpr";//FileNameGenerater.GenerateFilename(sender);+ string.Join(",", sender.record.players.Select(u => u.playerName))
                    string text = ReplayDir + "\\" + str;
                    //Utilities.IfDirectoryNotExitThenCreate(Properties.Settings.Default.ReplayDir);创建文件夹
                    if (!Directory.Exists(ReplayDir))
                    {
                        Directory.CreateDirectory(ReplayDir);
                    }
                    if (File.Exists(text))
                    {
                        text = text.Insert(text.Length - 4, new System.Random().Next(9999).ToString());
                    }
                    sender.record.writeToFile(text);
                    log(DateTime.Now.ToString() + ":" + "Record Game Success - GameID:" + sender.record.gameId.ToString() + " & Platform:" + new string(sender.record.gamePlatform) + " & Reason:" + reason);
                }
                //else
                //log(DateTime.Now.ToString() + ":" + "Record Game Failed - GameID:" + sender.record.gameId.ToString() + " & Platform:" + new string(sender.record.gamePlatform) + " & Reason:" + reason);
            }
            catch { }
        }

        private void insertSQL()
        {
            while (work)
            {
                SetFile();

                Thread.Sleep(600000);
            }
        }

        private void SetFile()
        {
            string ReplayDir = "d:\\replay";
            foreach (FileInfo file in new DirectoryInfo(ReplayDir).GetFiles())
            {
                if (file.Name.EndsWith(".lpr"))
                {
                    try
                    {
                        LoLRecord record = new LoLRecord();
                        record.readFromFile(file.FullName, false);
                        ulong gameid = record.gameId;

                        int gid = Convert.ToInt32(record.gameId);
                        GameDetail gd = db.GameDetail.FirstOrDefault(u => u.gameId == gid);
                        if (gd == null)
                        {
                            gd = new GameDetail();
                            gd.gameId = gid;
                            gd.gameLength = Convert.ToInt32(record.gameLength);
                            gd.observers = new string(record.observerEncryptionKey);
                            gd.platformId = new string(record.gamePlatform);
                            //gd.mapId = fg.mapId;

                            gd.gameMode = record.gameStats.GameMode;
                            gd.gameType = record.gameStats.GameType;
                            //gd.gameTypeConfigId = fg.gameTypeConfigId; 
                            //gd.gameStartTime = Convert.ToInt64(aaa);
                            //gd.gameQueueConfigId = fg.gameQueueConfigId; 
                            gd.createtime = DateTime.Now;
                            db.GameDetail.AddObject(gd);

                            foreach (var item in record.gameStats.Players)
                            {
                                participantsDetail pd = new participantsDetail();
                                pd.ID = Guid.NewGuid();
                                pd.gameId = gd.gameId;
                                pd.summonerName = item.SummonerName;
                                pd.championId = Utilities.GetChampionidByName(item.SkinName);
                                pd.spell1Id = item.Spell1Id;
                                pd.spell2Id = item.Spell2Id;
                                pd.teamId = Convert.ToInt32(item.TeamId);
                                //pd.skinIndex = item.SkinName;
                                pd.bot = item.BotPlayer;
                                pd.createtime = DateTime.Now;
                                db.participantsDetail.AddObject(pd);
                            }
                            db.SaveChanges();

                        }
                        string rd = ReplayDir + "\\" + gd.platformId;
                        if (!Directory.Exists(rd))
                        {
                            Directory.CreateDirectory(rd);
                        }
                        File.Copy(file.FullName, rd + "\\" + file.Name, true);
                        File.Delete(file.FullName);
                    }
                    catch (Exception ex) { }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            StartRecord();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopRecord();
        }
        private object obj = new object();
        private void log(string text)
        {
            lock (obj)
            {

                status.Add(text);
                //StreamWriter sw = new StreamWriter("v:\\log.txt", true, Encoding.Default);
                //sw.WriteLine(text);
                //sw.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lock (obj2)
            {
                listBox1.DataSource = null;
                if (LoLRecorder.Recorders != null)
                    listBox1.DataSource = LoLRecorder.Recorders.Select(u => u.record.gameId.ToString() + " " + new string(u.record.gamePlatform)).ToList();
                //MessageBox.Show("All:" + this.Games.Count() + "  Success:" + this.Recorders.Count());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            work = true;
            Utilities.GetNAServerInfo();
            SetFile();
            //status = new List<string>();
            //GC.Collect();
            //this.Games = new List<GameInfo>();
            //SearchHF();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int count = 0;
            //foreach (var item in db.Player.Where(u => u.platformid == "HN1_NEW" & (u.DWServerName != "1" && u.DWServerName != "0")))
            //{
            //    WebClient _httpClient = new WebClient();
            //    string result = _httpClient.DownloadString(string.Format("http://lolbox.duowan.com/ajaxGetWarzone.php?playerName={0}&serverName=%E7%94%B5%E4%BF%A1%E4%B8%80", item.Name));

            //    JObject jObject = JsonConvert.DeserializeObject<JObject>(result);
            //    string tier = jObject["tier"].ToString();
            //    string rank = jObject["rank"].ToString();
            //    db.ObjectStateManager.ChangeObjectState(item, EntityState.Modified);
            //    if (tier == "最强王者" || (tier == "钻石" && rank == "I"))
            //    {
            //        item.DWServerName = "1";
            //    }
            //    else
            //    {
            //        item.DWServerName = "0";
            //        count++;
            //    }
            //}
            //db.SaveChanges();
            //MessageBox.Show("选手清除OK" + count.ToString());
            //count = 0;
            DateTime dt = DateTime.Now.AddDays(-3);
            DateTime dt2 = DateTime.Now.AddHours(-12);
            foreach (var item in db.GameDetail.Where(u => u.createtime < dt && u.platformId != "HN1_NEW"))
            {

                count++;
                try
                {
                    db.participantsDetail.Delete(u => u.gameId == item.gameId);
                    File.Delete("d:\\replay\\" + item.platformId + "\\" + item.gameId + ".lpr");
                }
                catch (Exception ex)
                { string aaa = ex.Message; }
            }
            db.GameDetail.Delete(u => u.createtime < dt && u.platformId != "HN1_NEW");
            db.SaveChanges();

            foreach (var item in db.GameDetail.Where(u => u.createtime < dt2 && u.platformId == "HN1_NEW"))
            {

                count++;
                try
                {
                    db.participantsDetail.Delete(u => u.gameId == item.gameId);
                    File.Delete("d:\\replay\\" + item.platformId + "\\" + item.gameId + ".lpr");
                }
                catch (Exception ex)
                { string aaa = ex.Message; MessageBox.Show(aaa); }
            }
            db.GameDetail.Delete(u => u.createtime < dt2 && u.platformId == "HN1_NEW");
            db.SaveChanges();
            MessageBox.Show("文件删除OK" + count.ToString());

        }
        public void ClearData()
        {
            int count = 0;
            DateTime dt = DateTime.Now.AddDays(-3);
            DateTime dt2 = DateTime.Now.AddHours(-12);
            foreach (var item in db.GameDetail.Where(u => (u.createtime < dt && u.platformId != "HN1_NEW") || (u.createtime < dt2 && u.platformId == "HN1_NEW")))
            {

                count++;
                try
                {
                    db.participantsDetail.Delete(u => u.gameId == item.gameId);
                    File.Delete("d:\\replay\\" + item.platformId + "\\" + item.gameId + ".lpr");
                }
                catch (Exception ex)
                { string aaa = ex.Message; }
            }
            db.GameDetail.Delete(u => u.createtime < dt);
            db.SaveChanges();
        }
        public static string UnicodeToGB(string text)
        {
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(text, "\\\\u([\\w]{4})");
            if (mc != null && mc.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match m2 in mc)
                {
                    string v = m2.Value;
                    string word = v.Substring(2);
                    byte[] codes = new byte[2];
                    int code = Convert.ToInt32(word.Substring(0, 2), 16);
                    int code2 = Convert.ToInt32(word.Substring(2), 16);
                    codes[0] = (byte)code2;
                    codes[1] = (byte)code;
                    text = text.Replace(v, Encoding.Unicode.GetString(codes));
                }
            }
            else
            {

            }
            return text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

            lock (obj2)
            {
                listBox1.DataSource = null;
                listBox1.DataSource = status;
            }
        }
    }



}