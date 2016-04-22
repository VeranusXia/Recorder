
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ReplayDll;
namespace Recorder
{
    public class LoLRecorder
    {
        public string filePath;
        public int chunkCount;
        public int startupChunkEnd;
        public int gameChunkStart;
        public int chunkGot;
        public int keyframeGot;
        public int keyframeCount;
        private System.Collections.Generic.Dictionary<int, byte[]> keyFrames;
        private System.Collections.Generic.Dictionary<int, byte[]> chunks;
        public LoLRecord record;
        public bool selfGame;
        public PlayerInfo selfPlayerInfo;
        public string platformAddress;
        public string specAddressPrefix;
        public static System.Collections.Generic.List<LoLRecorder> Recorders;
        public event Program.RecordDoneDelegate doneEvent;


        public LoLRecorder(GameInfo gameinfo)
        {
            if (LoLRecorder.Recorders == null)
            {
                LoLRecorder.Recorders = new System.Collections.Generic.List<LoLRecorder>();
            }
            this.record = new LoLRecord();
            this.record.gameId = gameinfo.GameId;
            this.record.observerEncryptionKey = gameinfo.ObKey.ToCharArray();
            this.record.gamePlatform = gameinfo.PlatformId.ToCharArray();
            this.setPlatformAddress(gameinfo.ServerAddress);
            this.selfGame = false;
            //this.setPlatformAddress(Utilities.LoLObserveServersIpMapping[new string(this.record.gamePlatform)]);


        }
        public void setPlatformAddress(string url)
        {
            this.platformAddress = url;
            this.specAddressPrefix = "http://" + this.platformAddress + "/observer-mode/rest/consumer/";
        }

   
        public void StopRecording(bool isSuccess, string reason)
        {
            lock (obj)
            {
               // LoLRecorder.Recorders.Remove(this);
            }
            this.doneEvent(this, isSuccess, reason);
        }

        private static object obj = new object();
        private bool CheckDuplicateRecorder()
        {

            lock (obj)
            {
                if (LoLRecorder.Recorders.FirstOrDefault(k => k.platformAddress == this.platformAddress && k.record.gameId == this.record.gameId) != null)
                    return false;

                LoLRecorder.Recorders.Add(this);
                return true;
            }
        }
        public void startRecording()
        {

            try
            {
                if (!this.CheckDuplicateRecorder())
                {
                    Thread.Sleep(100000);
                    this.StopRecording(false, "");
                    return;
                }

                Console.WriteLine("Recording gameId:" + this.record.gameId.ToString());
                this.record.lolVersion = "4.0.0".ToCharArray();
                this.record.spectatorClientVersion = this.getUrlString(this.specAddressPrefix + "version").ToCharArray();
                if (!this.waitForSpectatorStart())
                {
                    this.StopRecording(false, "ErrorGettingSpecMode");
                    return;
                }
                if (!this.getInitBlocksNumber())
                {
                    this.StopRecording(false, "ErrorGettingSpecMode");
                    return;
                }
                if (!this.getGameContentFromServer())
                {
                    //this.StopRecording(false, "ErrorGettingContent");
                    //return;
                }
                JObject jObject = this.getGameMeta();
                if (jObject == null)
                {
                    //this.StopRecording(false, "ErrorGettingContent");
                    //return;
                }
                try
                {
                    this.record.gameMetaAnalyze(jObject);

                }
                catch
                {
                    //this.StopRecording(false, "ErrorGettingContent");
                }
                jObject = this.getLastChunkInfo();
                if (jObject == null)
                {
                    //this.StopRecording(false, "ErrorGettingContent");
                    //return;
                }
                this.record.lastChunkInfoAnalyze(jObject);
                this.record.setAllChunksContent(this.chunks);
                this.record.setAllKeyFrameContent(this.keyFrames);
                this.record.setEndOfGameStats(this.getEndOfGameStats());


                this.StopRecording(true, string.Empty);
            }
            catch { this.StopRecording(false, string.Empty); }

        }


        private byte[] getEndOfGameStats()
        {
            string urlString = this.getUrlString(string.Concat(new object[]
			{
				this.specAddressPrefix,
				"endOfGameStats/",
				new string(this.record.gamePlatform),
				"/",
				this.record.gameId,
				"/null"
			}));
            byte[] result;
            try
            {
                result = System.Convert.FromBase64String(urlString.Replace(" ", "+"));
            }
            catch
            {
                return null;
            }
            return result;
        }
        private JObject getUrlJson(string url)
        {
            JObject result = null;
            try
            {
                result = JsonConvert.DeserializeObject<JObject>(this.getUrlString(url));
            }
            catch (System.Exception)
            {
                return null;
            }
            return result;
        }
        private string getUrlString(string url)
        {
            string text = null;
            using (WebClient webClient = new WebClient())
            {
                int num = 0;
                do
                {
                    try
                    {
                        text = webClient.DownloadString(url);
                    }
                    catch (WebException)
                    {
                        if (num++ >= 3)
                        {
                            return null;
                        }
                        System.Threading.Thread.Sleep(10000);
                    }
                }
                while (text == null);
            }
            return text;
        }
        private bool getInitBlocksNumber()
        {
            JObject urlJson = this.getUrlJson(string.Concat(new object[]
			{
				this.specAddressPrefix,
				"getGameMetaData/",
				new string(this.record.gamePlatform),
				"/",
				this.record.gameId,
				"/1/token"
			}));
            if (urlJson == null)
            {
                return false;
            }
            this.chunkCount = int.Parse(urlJson["lastChunkId"].ToString());
            this.startupChunkEnd = int.Parse(urlJson["endStartupChunkId"].ToString());
            this.gameChunkStart = int.Parse(urlJson["startGameChunkId"].ToString());
            return true;
        }
        private bool waitForSpectatorStart()
        {
            if (!this.selfGame)
            {
                return true;
            }
            int num = 0;
            System.DateTime now = System.DateTime.Now;
            //do
            {
                System.Threading.Thread.Sleep(10000);
                JObject urlJson = this.getUrlJson(string.Concat(new object[]
				{
					this.specAddressPrefix,
					"getLastChunkInfo/",
					new string(this.record.gamePlatform),
					"/",
					this.record.gameId,
					"/30000/token"
				}));
                try
                {
                    num = int.Parse(urlJson["nextAvailableChunk"].ToString());
                }
                catch
                {
                    if ((System.DateTime.Now - now).TotalMinutes > 3.0)
                    {
                        return false;
                    }
                }
            }
            //while (num < 0);
            System.Threading.Thread.Sleep(num);
            return true;
        }
        private bool getGameContentFromServer()
        {
            this.chunks = new System.Collections.Generic.Dictionary<int, byte[]>();
            this.keyFrames = new System.Collections.Generic.Dictionary<int, byte[]>();
            this.chunkGot = 1;
            this.keyframeGot = 1;
            while (true)
            {

                JObject urlJson = this.getUrlJson(string.Concat(new object[]
				{
					this.specAddressPrefix,
					"getLastChunkInfo/",
					new string(this.record.gamePlatform),
					"/",
					this.record.gameId,
					"/30000/token"
				}));
                if (urlJson == null)
                {
                    break;
                }
                this.chunkCount = int.Parse(urlJson["chunkId"].ToString());
                this.keyframeCount = int.Parse(urlJson["keyFrameId"].ToString());
                int num = int.Parse(urlJson["nextAvailableChunk"].ToString());
                this.chunkGot = 1;
                System.Threading.Thread.Sleep(num);
                if (!this.getGameChunksFromServer())
                {
                    return false;
                }
                if (!this.getKeyFramesFromServer())
                {
                    return false;
                }
                if (num == 0)
                {
                    return true;
                }
            }
            return false;
        }
        public JObject getGameMeta()
        {
            return this.getUrlJson(string.Concat(new object[]
			{
				this.specAddressPrefix,
				"getGameMetaData/",
				new string(this.record.gamePlatform),
				"/",
				this.record.gameId,
				"/1/token"
			}));
        }
        public JObject getLastChunkInfo()
        {
            return this.getUrlJson(string.Concat(new object[]
			{
				this.specAddressPrefix,
				"getLastChunkInfo/",
				new string(this.record.gamePlatform),
				"/",
				this.record.gameId,
				"/30000/token"
			}));
        }
        private bool getNthBlockAndWrite(int n, bool IsKeyFrame)
        {
            if (IsKeyFrame)
            {
                if (this.keyFrames.ContainsKey(n))
                {
                    return true;
                }
            }
            else
            {
                if (this.chunks.ContainsKey(n))
                {
                    return true;
                }
            }
            WebClient webClient = new WebClient();
            int num = 0;
            byte[] value;
        IL_08:
            try
            {
                if (IsKeyFrame)
                {
                    value = webClient.DownloadData(string.Concat(new object[]
					{
						this.specAddressPrefix,
						"getKeyFrame/",
						new string(this.record.gamePlatform),
						"/",
						this.record.gameId,
						"/",
						n,
						"/token"
					}));
                }
                else
                {
                    value = webClient.DownloadData(string.Concat(new object[]
					{
						this.specAddressPrefix,
						"getGameDataChunk/",
						new string(this.record.gamePlatform),
						"/",
						this.record.gameId,
						"/",
						n,
						"/token"
					}));
                }
            }
            catch (WebException)
            {
                if (num++ < 3)
                {
                    System.Threading.Thread.Sleep(200);
                    goto IL_08;
                }
                else
                {
                    return true;
                }
                return false;
            }
            if (IsKeyFrame)
            {
                if (!this.keyFrames.ContainsKey(n))
                {
                    this.keyFrames.Add(n, value);
                }
            }
            else
            {
                if (!this.chunks.ContainsKey(n))
                {
                    this.chunks.Add(n, value);
                }
            }
            return true;
        }
        private bool getGameChunksFromServer()
        {
            while (this.chunkGot <= this.chunkCount)
            {
                if (!this.getNthBlockAndWrite(this.chunkGot, false))
                {
                    return false;
                }
                this.chunkGot++;
            }
            return true;
        }
        private bool getKeyFramesFromServer()
        {
            while (this.keyframeGot <= this.keyframeCount)
            {
                if (!this.getNthBlockAndWrite(this.keyframeGot, true))
                {
                    return false;
                }
                this.keyframeGot++;
            }
            return true;
        }
    }
}
