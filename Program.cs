using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReplayDll;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace Recorder
{
    public class Program
    { 
        public static void Main(string[] args)
        {
            try
            {
                GameInfo g = new GameInfo();
                while (true)
                {
                    g = AnalyzeWhetherSpectatorMode();
                    if (g != null)
                    {
                        StartNewRecoding(g);

                    }
                    Thread.Sleep(10000);
                }
            }
            catch (Exception ex) { Console.Write(ex.Message); }
        }

        public delegate void RecordDoneDelegate(LoLRecorder sender, bool isSuccess, string reason);
        public static void RecordDone(LoLRecorder sender, bool isSuccess, string reason)
        {

            if (isSuccess)
            {
                try
                {
                    string ReplayDir = System.Environment.CurrentDirectory + "\\Replay";
                    string str = sender.record.gameId + ".lpr";
                    string text = ReplayDir + "\\" + str;
                    if (!Directory.Exists(ReplayDir))
                    {
                        Directory.CreateDirectory(ReplayDir);
                    }
                    if (File.Exists(text))
                    {
                        return;
                    }
                    sender.record.writeToFile(text);
                    sender.record.writeResultToFile(ReplayDir + "\\" + sender.record.gameId + "_result.json");

                    Console.WriteLine("Success:" + sender.record.gameId.ToString());
                }
                catch (Exception ex)
                {

                }
            }
            else {

            }
        }
 
        public static void StartNewRecoding(GameInfo g)
        {
            LoLRecorder r = new LoLRecorder(g);

            r.doneEvent += RecordDone;
            try
            {
                r.startRecording();
            }
            catch
            {
                Console.Write("Error:" + JsonConvert.SerializeObject(g));
            }
        }

        private static GameInfo AnalyzeWhetherSpectatorMode()
        {
            Thread.Sleep(1000);
            GameInfo g = new GameInfo();
            string gameLog = GetLOLCommandLines();
            if (gameLog.Contains("spectator"))
            {
                Regex regex = new Regex("spectator (?<ADDR>[A-Za-z0-9\\.-]+:[0-9]*) (?<key>.+) (?<GID>[a-f0-9]+) (?<PID>[A-Z0-9_]+)", RegexOptions.IgnoreCase);
                Match match = regex.Match(gameLog);
                if (match.Success)
                {
                    g.ServerAddress = match.Groups["ADDR"].Value;
                    g.PlatformId = match.Groups["PID"].Value;
                    g.ObKey = match.Groups["key"].Value;
                    g.GameId = ulong.Parse(match.Groups["GID"].Value);
                    if (g.ServerAddress.StartsWith("127."))
                        return null;
                    return g;
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        private static string GetLOLCommandLines()
        {

            try
            {
                System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Process");
                string command = "";
                foreach (System.Management.ManagementObject disk in searcher.Get())
                {
                    if (disk["Name"].ToString() == "League of Legends.exe")
                    {
                        command = disk["commandline"].ToString().Replace("\"", "");
                    }
                }
                return command;
            }
            catch { return null; }

        }
    }
}
