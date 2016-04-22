using System;
namespace RecordCode
{
	public class PlayerInfo
	{
		public string playerName;
		public string championName;
		public uint team;
		public int clientID;
		public PlayerInfo(string pName, string cName, uint team, int cId)
		{
			this.playerName = pName;
			this.championName = cName;
			this.team = team;
			this.clientID = cId;
		}
		public PlayerInfo()
		{
		}
	}
}
