using System;
namespace RecordCode
{
	public class GameInfo
	{
		public string ServerAddress
		{
			get;
			set;
		}
		public string ObKey
		{
			get;
			set;
		}
		public ulong GameId
		{
			get;
			set;
		}
		public string PlatformId
		{
			get;
			set;
		}
		public GameInfo(string obUrl, string platform, ulong gameId, string obKey)
		{
			this.ServerAddress = obUrl;
			this.ObKey = obKey;
			this.GameId = gameId;
			this.PlatformId = platform;
		}
		public GameInfo()
		{
		}
	}
}
