
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
namespace RecordCode
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
        public event Form1.RecordDoneDelegate doneEvent;


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
            //this.setPlatformAddress(Utilities.LoLObserveServersIpMapping[new string(this.record.gamePlatform)]);


        }
        public void setPlatformAddress(string url)
        {
            this.platformAddress = url;
            this.specAddressPrefix = "http://" + this.platformAddress + "/observer-mode/rest/consumer/";
        }

        private bool analyzeObKeyInMemoryBytesMethod1(byte[] memBytes)
        {
            try
            {
                int i = 0;
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                bool flag = false;
                while (i < memBytes.Length)
                {
                    if (memBytes[i] > 32 && memBytes[i] < 125)
                    {
                        stringBuilder.Append((char)memBytes[i]);
                        flag = false;
                    }
                    else
                    {
                        if (!flag)
                        {
                            stringBuilder.Append(' ');
                            flag = true;
                        }
                    }
                    i++;
                }
                string[] array = stringBuilder.ToString().Split(new char[]
				{
					' '
				});
                string[] array2 = array;
                for (int j = 0; j < array2.Length; j++)
                {
                    string text = array2[j];
                    if (text.Length == 33 && text[0] == 'A')
                    {
                        this.record.observerEncryptionKey = text.Substring(1, 32).ToCharArray();
                        bool result = true;
                        return result;
                    }
                }
            }
            catch
            {
                bool result = false;
                return result;
            }
            return false;
        }
        public void StopRecording(bool isSuccess, string reason)
        {
            lock (obj)
            {
                LoLRecorder.Recorders.Remove(this);
            }
            this.doneEvent(this, isSuccess, reason);
        }

        private static object obj = new object();
        private bool CheckDuplicateRecorder()
        {
            //foreach (LoLRecorder current in LoLRecorder.Recorders)
            //{
            //    if (this.record.gameId == current.record.gameId && this.platformAddress == current.platformAddress)
            //    {
            //        return false;
            //    }
            //}
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
                    this.StopRecording(false, "ErrorDuplicateGame");
                    return;
                }
                this.record.lolVersion = "3.15".ToCharArray();
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
                    this.StopRecording(false, "ErrorGettingContent");
                    return;
                }
                JObject jObject = this.getGameMeta();
                if (jObject == null)
                {
                    this.StopRecording(false, "ErrorGettingContent");
                    return;
                }
                try
                {
                    this.record.gameMetaAnalyze(jObject);
                }
                catch
                {
                    this.StopRecording(false, "ErrorGettingContent");
                }
                jObject = this.getLastChunkInfo();
                if (jObject == null)
                {
                    this.StopRecording(false, "ErrorGettingContent");
                    return;
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
            do
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
            while (num < 0);
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
            WebClient webClient = new WebClient();
            int num = 0;
            int num1 = 0;
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
                    this.keyframeGot = n + 1;
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
                    this.chunkGot = n + 1;
                }
                num = 0;
            }
            catch (WebException)
            {

                if (num++ < 30)
                {
                    System.Threading.Thread.Sleep(5000);
                    n++;
                    goto IL_08;
                }
                else
                {
                    if (num1++ < 5)
                    {
                        System.Threading.Thread.Sleep(5000);
                        n = 0;
                        goto IL_08;
                    }
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
                //this.chunkGot++;
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
                //this.keyframeGot++;
            }
            return true;
        }
    }
}
