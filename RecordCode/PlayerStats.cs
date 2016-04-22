using FluorineFx;
using FluorineFx.AMF3;
using System;
namespace RecordCode
{
	public class PlayerStats
	{
		protected string _skinName;
		protected bool _botPlayer;
		protected ulong _userId;
		protected ulong _gameId;
		protected string _summonerName;
		protected bool _leaver;
		protected uint _teamId;
		protected int _spell1Id;
		protected int _spell2Id;
		private DetailStats _detailStats;
		public string SkinName
		{
			get
			{
				return this._skinName;
			}
			set
			{
				this._skinName = value;
			}
		}
		public bool BotPlayer
		{
			get
			{
				return this._botPlayer;
			}
			set
			{
				this._botPlayer = value;
			}
		}
		public ulong UserId
		{
			get
			{
				return this._userId;
			}
			set
			{
				this._userId = value;
			}
		}
		public ulong GameId
		{
			get
			{
				return this._gameId;
			}
			set
			{
				this._gameId = value;
			}
		}
		public string SummonerName
		{
			get
			{
				return this._summonerName;
			}
			set
			{
				this._summonerName = value;
			}
		}
		public bool Leaver
		{
			get
			{
				return this._leaver;
			}
			set
			{
				this._leaver = value;
			}
		}
		public uint TeamId
		{
			get
			{
				return this._teamId;
			}
			set
			{
				this._teamId = value;
			}
		}
		public int Spell1Id
		{
			get
			{
				return this._spell1Id;
			}
			set
			{
				this._spell1Id = value;
			}
		}
		public int Spell2Id
		{
			get
			{
				return this._spell2Id;
			}
			set
			{
				this._spell2Id = value;
			}
		}
		public DetailStats DetailStats
		{
			get
			{
				return this._detailStats;
			}
			set
			{
				this._detailStats = value;
			}
		}
		public PlayerStats()
		{
		}
		public PlayerStats(ASObject pStats)
		{
			this.SkinName = (pStats["skinName"] as string);
			this.BotPlayer = (bool)pStats["botPlayer"];
			this.UserId = ulong.Parse(pStats["userId"].ToString());
			this.GameId = ulong.Parse(pStats["gameId"].ToString());
			this.SummonerName = pStats["summonerName"].ToString();
			this.Leaver = (bool)pStats["leaver"];
			this.TeamId = uint.Parse(pStats["teamId"].ToString());
			this.Spell1Id = int.Parse(pStats["spell1Id"].ToString());
			this.Spell2Id = int.Parse(pStats["spell2Id"].ToString());
			this.DetailStats = new DetailStats(pStats["statistics"] as ArrayCollection);
		}
	}
}
