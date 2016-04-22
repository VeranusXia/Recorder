using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
namespace RecordCode
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
		public string getMetaData()
		{
			JObject jObject = new JObject();
			jObject.Add("gameId", this.gameId);
			jObject.Add("platformId", new string(this.gamePlatform));
			return new JObject
			{

				{
					"gameKey",
					jObject
				},

				{
					"gameServerAddress",
					""
				},

				{
					"port",
					0
				},

				{
					"encryptionKey",
					""
				},

				{
					"chunkTimeInterval",
					this.gameChunkTimeInterval
				},

				{
					"startTime",
					new string(this.gameStartTime)
				},

				{
					"endTime",
					new string(this.gameEndTime)
				},

				{
					"gameEnded",
					true
				},

				{
					"lastChunkId",
					this.gameEndChunkId
				},

				{
					"lastKeyFrameId",
					this.gameEndKeyFrameId
				},

				{
					"endStartupChunkId",
					this.gameEndStartupChunkId
				},

				{
					"delayTime",
					this.gameDelayTime
				},

				{
					"pendingAvailableChunkInfo",
					""
				},

				{
					"pendingAvailableKeyFrameInfo",
					""
				},

				{
					"keyFrameTimeInterval",
					this.gameKeyFrameTimeInterval
				},

				{
					"decodedEncryptionKey",
					""
				},

				{
					"startGameChunkId",
					this.gameStartChunkId
				},

				{
					"gameLength",
					this.gameLength
				},

				{
					"clientAddedLag",
					this.gameClientAddLag
				},

				{
					"clientBackFetchingEnabled",
					true
				},

				{
					"clientBackFetchingFreq",
					"50"
				},

				{
					"interestScore",
					this.gameELOLevel
				},

				{
					"featuredGame",
					"false"
				},

				{
					"createTime",
					new string(this.gameCreateTime)
				}
			}.ToString();
		}
		public string getLastChunkInfo()
		{
			return new JObject
			{

				{
					"chunkId",
					this.gameEndChunkId
				},

				{
					"availableSince",
					this.gameLastChunkTime
				},

				{
					"nextAvailableChunk",
					0
				},

				{
					"keyFrameId",
					this.gameEndKeyFrameId
				},

				{
					"nextChunkId",
					this.gameEndChunkId
				},

				{
					"endStartupChunkId",
					this.gameEndStartupChunkId
				},

				{
					"startGameChunkId",
					this.gameStartChunkId
				},

				{
					"endGameChunkId",
					this.gameEndChunkId
				},

				{
					"duration",
					this.gameLastChunkDuration
				}
			}.ToString();
		}
		private void allocateChunkAndKeyFrameSpaces()
		{
			this.gameKeyFrames = new System.Collections.Generic.Dictionary<int, byte[]>();
			this.gameChunks = new System.Collections.Generic.Dictionary<int, byte[]>();
		}
		public void setKeyFrameContent(int n, byte[] contentBytes)
		{
			if (this.gameKeyFrames.ContainsKey(n))
			{
				this.gameKeyFrames.Remove(n);
			}
			this.gameKeyFrames.Add(n, contentBytes);
		}
		public byte[] getKeyFrameContent(int n)
		{
			if (this.gameKeyFrames.ContainsKey(n))
			{
				return this.gameKeyFrames[n];
			}
			return null;
		}
		public void setChunkContent(int n, byte[] contentBytes)
		{
			if (this.gameChunks.ContainsKey(n))
			{
				this.gameChunks.Remove(n);
			}
			this.gameChunks.Add(n, contentBytes);
		}
		public byte[] getChunkContent(int n)
		{
			if (this.gameChunks.ContainsKey(n))
			{
				return this.gameChunks[n];
			}
			return null;
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
		public void readFromFile(string path, bool withOutChunks)
		{
			this.relatedFileName = path;
			try
			{
				System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(fileStream);
				this.ThisLPRVersion = binaryReader.ReadInt32();
				if (this.ThisLPRVersion >= 0)
				{
					int count = binaryReader.ReadInt32();
					this.spectatorClientVersion = binaryReader.ReadChars(count);
					if (this.ThisLPRVersion < 2)
					{
						this.gameId = (ulong)((long)binaryReader.ReadInt32());
					}
					else
					{
						this.gameId = binaryReader.ReadUInt64();
					}
					this.gameEndStartupChunkId = binaryReader.ReadInt32();
					this.gameStartChunkId = binaryReader.ReadInt32();
					this.gameEndChunkId = binaryReader.ReadInt32();
					this.gameEndKeyFrameId = binaryReader.ReadInt32();
					this.gameLength = binaryReader.ReadInt32();
					this.gameDelayTime = binaryReader.ReadInt32();
					this.gameClientAddLag = binaryReader.ReadInt32();
					this.gameChunkTimeInterval = binaryReader.ReadInt32();
					this.gameKeyFrameTimeInterval = binaryReader.ReadInt32();
					this.gameELOLevel = binaryReader.ReadInt32();
					this.gameLastChunkTime = binaryReader.ReadInt32();
					this.gameLastChunkDuration = binaryReader.ReadInt32();
					count = binaryReader.ReadInt32();
					this.gamePlatform = binaryReader.ReadChars(count);
					count = binaryReader.ReadInt32();
					this.observerEncryptionKey = binaryReader.ReadChars(count);
					count = binaryReader.ReadInt32();
					this.gameCreateTime = binaryReader.ReadChars(count);
					count = binaryReader.ReadInt32();
					this.gameStartTime = binaryReader.ReadChars(count);
					count = binaryReader.ReadInt32();
					this.gameEndTime = binaryReader.ReadChars(count);
					if (this.ThisLPRVersion >= 3)
					{
						count = binaryReader.ReadInt32();
						this.lolVersion = binaryReader.ReadChars(count);
					}
					else
					{
						this.lolVersion = string.Empty.ToCharArray();
					}
					if (this.ThisLPRVersion >= 2)
					{
						this.hasResult = binaryReader.ReadBoolean();
						if (this.ThisLPRVersion >= 4)
						{
							if (this.hasResult)
							{
								count = binaryReader.ReadInt32();
								this.endOfGameStatsBytes = binaryReader.ReadBytes(count);
								this.gameStats = new EndOfGameStats(this.endOfGameStatsBytes);
							}
							if (binaryReader.ReadBoolean())
							{
								this.readPlayerOldFormat(binaryReader);
							}
						}
						else
						{
							if (this.hasResult)
							{
								count = binaryReader.ReadInt32();
								this.endOfGameStatsBytes = binaryReader.ReadBytes(count);
								this.gameStats = new EndOfGameStats(this.endOfGameStatsBytes);
							}
							else
							{
								this.readPlayerOldFormat(binaryReader);
							}
						}
					}
					else
					{
						this.readPlayerOldFormat(binaryReader);
					}
					if (!withOutChunks)
					{
						this.readChunks(binaryReader);
					}
					binaryReader.Close();
					fileStream.Close();
				}
			}
			catch
			{
				this.IsBroken = true;
			}
		}
		public void readPlayerOldFormat(System.IO.BinaryReader dataReader)
		{
			this.players = new PlayerInfo[dataReader.ReadInt32()];
			for (int i = 0; i < this.players.Length; i++)
			{
				this.players[i] = new PlayerInfo();
				int count = dataReader.ReadInt32();
				char[] value = dataReader.ReadChars(count);
				this.players[i].playerName = new string(value);
				count = dataReader.ReadInt32();
				char[] value2 = dataReader.ReadChars(count);
				this.players[i].championName = new string(value2);
				this.players[i].team = dataReader.ReadUInt32();
				this.players[i].clientID = dataReader.ReadInt32();
			}
		}
		private void readChunks(System.IO.BinaryReader dataReader)
		{
			this.allocateChunkAndKeyFrameSpaces();
			if (this.ThisLPRVersion == 0)
			{
				this.readChunksVersion0(dataReader);
				return;
			}
			int num = dataReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int key = dataReader.ReadInt32();
				int count = dataReader.ReadInt32();
				this.gameKeyFrames.Add(key, dataReader.ReadBytes(count));
			}
			num = dataReader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				int key2 = dataReader.ReadInt32();
				int count2 = dataReader.ReadInt32();
				this.gameChunks.Add(key2, dataReader.ReadBytes(count2));
			}
		}
		private void readChunksVersion0(System.IO.BinaryReader dataReader)
		{
			for (int i = 0; i < this.gameEndKeyFrameId; i++)
			{
				int count = dataReader.ReadInt32();
				this.gameKeyFrames.Add(i + 1, dataReader.ReadBytes(count));
			}
			for (int j = 0; j < this.gameEndStartupChunkId; j++)
			{
				int count = dataReader.ReadInt32();
				this.gameChunks.Add(j + 1, dataReader.ReadBytes(count));
			}
			for (int k = 0; k <= this.gameEndChunkId - this.gameStartChunkId; k++)
			{
				int count = dataReader.ReadInt32();
				this.gameChunks.Add(this.gameStartChunkId + k, dataReader.ReadBytes(count));
			}
		}
		private static string[] ReadGameStatsChampion(EndOfGameStats egs)
		{
			string[] array = new string[10];
			int num = 5;
			int num2 = 4;
			if (egs.Players.Count > 0)
			{
				for (int i = 0; i < egs.Players.Count; i++)
				{
					if (egs.Players[i].TeamId == 100u)
					{
						array[num2--] = egs.Players[i].SkinName;
					}
					else
					{
						array[num++] = egs.Players[i].SkinName;
					}
				}
			}
			return array;
		}
		private static string[] ReadGameStatsSummonerName(EndOfGameStats egs)
		{
			string[] array = new string[10];
			int num = 5;
			int num2 = 4;
			if (egs.Players.Count > 0)
			{
				for (int i = 0; i < egs.Players.Count; i++)
				{
					if (egs.Players[i].TeamId == 100u)
					{
						array[num2--] = egs.Players[i].SummonerName;
					}
					else
					{
						array[num++] = egs.Players[i].SummonerName;
					}
				}
			}
			return array;
		}
		private static string[] ReadLogChampion(PlayerInfo[] players)
		{
			string[] array = new string[10];
			int num = 5;
			int num2 = 4;
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].team == 100u)
				{
					array[num2--] = players[i].championName;
				}
				else
				{
					array[num++] = players[i].championName;
				}
			}
			return array;
		}
		private static string[] ReadLogSummonerName(PlayerInfo[] players)
		{
			string[] array = new string[10];
			int num = 5;
			int num2 = 4;
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].team == 100u)
				{
					array[num2--] = players[i].playerName;
				}
				else
				{
					array[num++] = players[i].playerName;
				}
			}
			return array;
		}
        //public static SimpleLoLRecord GetSimpleLoLRecord(string path)
        //{
        //    LoLRecord loLRecord = new LoLRecord();
        //    loLRecord.readFromFile(path, true);
        //    return LoLRecord.GetSimpleLoLRecord(loLRecord);
        //}
        //public static SimpleLoLRecord GetSimpleLoLRecord(LoLRecord record)
        //{
        //    if (record.IsBroken)
        //    {
        //        return null;
        //    }
        //    SimpleLoLRecord simpleLoLRecord = new SimpleLoLRecord();
        //    simpleLoLRecord.FileName = record.relatedFileName;
        //    try
        //    {
        //        if (record.players == null)
        //        {
        //            simpleLoLRecord.Champions = LoLRecord.ReadGameStatsChampion(record.gameStats);
        //            simpleLoLRecord.SummonerName = LoLRecord.ReadGameStatsSummonerName(record.gameStats);
        //        }
        //        else
        //        {
        //            simpleLoLRecord.Champions = LoLRecord.ReadLogChampion(record.players);
        //            simpleLoLRecord.SummonerName = LoLRecord.ReadLogSummonerName(record.players);
        //        }
        //    }
        //    catch (System.Exception)
        //    {
        //        return null;
        //    }
        //    return simpleLoLRecord;
        //}
	}
}
