using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using ReplayDll;
using System.Text;
using FluorineFx.IO;
using FluorineFx;
namespace Recorder
{
    public class LoLRecord
    {
        public const int LPRLatestVersion = 4;
        public bool IsBroken;
        public int ThisLPRVersion = 4;
        public string relatedFileName;
        public char[] spectatorClientVersion;
        public ulong gameId;
        public int gameChunkTimeInterval;
        public int gameKeyFrameTimeInterval;
        public int gameStartChunkId;
        public int gameEndKeyFrameId;
        public int gameEndChunkId;
        public int gameEndStartupChunkId;
        public int gameLength;
        public int gameClientAddLag;
        public int gameELOLevel;
        public char[] gamePlatform;
        public char[] observerEncryptionKey;
        public char[] gameCreateTime;
        public char[] gameStartTime;
        public char[] gameEndTime;
        public int gameDelayTime;
        public int gameLastChunkTime;
        public int gameLastChunkDuration;
        public char[] lolVersion;
        public bool hasResult;
        public EndOfGameStats gameStats;
        public System.Collections.Generic.Dictionary<int, byte[]> gameChunks;
        public System.Collections.Generic.Dictionary<int, byte[]> gameKeyFrames;
        private byte[] endOfGameStatsBytes;

        public PlayerInfo[] players;
        public void gameMetaAnalyze(JObject metaJson)
        {
            JObject jObject = JsonConvert.DeserializeObject<JObject>(metaJson["gameKey"].ToString());
            this.gameId = ulong.Parse(jObject["gameId"].ToString());
            this.gamePlatform = jObject["platformId"].ToString().ToCharArray();
            this.gameChunkTimeInterval = int.Parse(metaJson["chunkTimeInterval"].ToString());
            this.gameStartTime = metaJson["startTime"].ToString().ToCharArray();
            this.gameEndTime = metaJson["endTime"].ToString().ToCharArray();
            this.gameEndChunkId = int.Parse(metaJson["lastChunkId"].ToString());
            this.gameEndKeyFrameId = int.Parse(metaJson["lastKeyFrameId"].ToString());
            this.gameEndStartupChunkId = int.Parse(metaJson["endStartupChunkId"].ToString());
            this.gameDelayTime = int.Parse(metaJson["delayTime"].ToString());
            this.gameKeyFrameTimeInterval = int.Parse(metaJson["keyFrameTimeInterval"].ToString());
            this.gameStartChunkId = int.Parse(metaJson["startGameChunkId"].ToString());
            this.gameLength = int.Parse(metaJson["gameLength"].ToString());
            this.gameClientAddLag = int.Parse(metaJson["clientAddedLag"].ToString());
            this.gameELOLevel = int.Parse(metaJson["interestScore"].ToString());
            this.gameCreateTime = metaJson["createTime"].ToString().ToCharArray();
            this.allocateChunkAndKeyFrameSpaces();
        }  
        private void allocateChunkAndKeyFrameSpaces()
        {
            this.gameKeyFrames = new System.Collections.Generic.Dictionary<int, byte[]>();
            this.gameChunks = new System.Collections.Generic.Dictionary<int, byte[]>();
        }
 
 
        public void setAllChunksContent(System.Collections.Generic.Dictionary<int, byte[]> chunks)
        {
            this.gameChunks = chunks;
        }
        public void setAllKeyFrameContent(System.Collections.Generic.Dictionary<int, byte[]> keyFrames)
        {
            this.gameKeyFrames = keyFrames;
        }
        public void lastChunkInfoAnalyze(JObject lastChunkJson)
        {
            this.gameLastChunkTime = int.Parse(lastChunkJson["availableSince"].ToString());
            this.gameLastChunkDuration = int.Parse(lastChunkJson["duration"].ToString());
        }
        public void setEndOfGameStats(byte[] content)
        {
            if (content == null)
            {
                this.hasResult = false;
                return;
            }
            this.endOfGameStatsBytes = content;
            this.gameStats = new EndOfGameStats(this.endOfGameStatsBytes);
            if (!this.gameStats.DecodeData())
            {
                this.hasResult = false;
                return;
            }
            if (this.gameStats.Players != null && this.gameStats.Players.Count > 0)
            {
                this.players = new PlayerInfo[this.gameStats.Players.Count];
                for (int i = 0; i < this.gameStats.Players.Count; i++)
                {
                    this.players[i] = new PlayerInfo();
                    this.players[i].championName = this.gameStats.Players[i].SkinName;
                    this.players[i].playerName = this.gameStats.Players[i].SummonerName;
                    this.players[i].team = this.gameStats.Players[i].TeamId;
                    this.players[i].clientID = i;
                }
            }
            this.hasResult = true;
        }
        public void writeToFile(string path)
        {
            System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fileStream);
            binaryWriter.Write(4);
            binaryWriter.Write(this.spectatorClientVersion.Length);
            binaryWriter.Write(this.spectatorClientVersion);
            binaryWriter.Write(this.gameId);
            binaryWriter.Write(this.gameEndStartupChunkId);
            binaryWriter.Write(this.gameStartChunkId);
            binaryWriter.Write(this.gameEndChunkId);
            binaryWriter.Write(this.gameEndKeyFrameId);
            binaryWriter.Write(this.gameLength);
            binaryWriter.Write(this.gameDelayTime);
            binaryWriter.Write(this.gameClientAddLag);
            binaryWriter.Write(this.gameChunkTimeInterval);
            binaryWriter.Write(this.gameKeyFrameTimeInterval);
            binaryWriter.Write(this.gameELOLevel);
            binaryWriter.Write(this.gameLastChunkTime);
            binaryWriter.Write(this.gameLastChunkDuration);
            binaryWriter.Write(this.gamePlatform.Length);
            binaryWriter.Write(this.gamePlatform);
            binaryWriter.Write(this.observerEncryptionKey.Length);
            binaryWriter.Write(this.observerEncryptionKey);
            binaryWriter.Write(this.gameCreateTime.Length);
            binaryWriter.Write(this.gameCreateTime);
            binaryWriter.Write(this.gameStartTime.Length);
            binaryWriter.Write(this.gameStartTime);
            binaryWriter.Write(this.gameEndTime.Length);
            binaryWriter.Write(this.gameEndTime);
            binaryWriter.Write(this.lolVersion.Length);
            binaryWriter.Write(this.lolVersion);
            binaryWriter.Write(this.hasResult);
            if (this.hasResult)
            {
                binaryWriter.Write(this.endOfGameStatsBytes.Length);
                binaryWriter.Write(this.endOfGameStatsBytes);
            }
            if (this.players != null)
            {
                binaryWriter.Write(true);
                binaryWriter.Write(this.players.Length);
                PlayerInfo[] array = this.players;
                for (int i = 0; i < array.Length; i++)
                {
                    PlayerInfo playerInfo = array[i];
                    char[] array2 = playerInfo.playerName.ToCharArray();
                    binaryWriter.Write(array2.Length);
                    binaryWriter.Write(array2);
                    char[] array3 = playerInfo.championName.ToCharArray();
                    binaryWriter.Write(array3.Length);
                    binaryWriter.Write(array3);
                    binaryWriter.Write(playerInfo.team);
                    binaryWriter.Write(playerInfo.clientID);
                }
            }
            else
            {
                binaryWriter.Write(false);
            }
            binaryWriter.Write(this.gameKeyFrames.Count);
            foreach (System.Collections.Generic.KeyValuePair<int, byte[]> current in this.gameKeyFrames)
            {
                binaryWriter.Write(current.Key);
                binaryWriter.Write(current.Value.Length);
                binaryWriter.Write(current.Value);
            }
            binaryWriter.Write(this.gameChunks.Count);
            foreach (System.Collections.Generic.KeyValuePair<int, byte[]> current2 in this.gameChunks)
            {
                binaryWriter.Write(current2.Key);
                binaryWriter.Write(current2.Value.Length);
                binaryWriter.Write(current2.Value);
            }
             

            binaryWriter.Close();
            fileStream.Close();
            this.relatedFileName = path;
        }
        public void writeResultToFile(string path)
        {

            System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fileStream);


            System.IO.Stream stream = new System.IO.MemoryStream(this.endOfGameStatsBytes);
            using (AMFReader aMFReader = new AMFReader(stream))
            {
                try
                {
                    ASObject aSObject = (ASObject)aMFReader.ReadAMF3Data();
                    sw.Write(JsonConvert.SerializeObject(aSObject));
                }
                catch { }
            }


            sw.Close();
            fileStream.Close();
        }

      
       
    }
}
