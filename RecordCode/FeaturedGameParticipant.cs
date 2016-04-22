using System;
namespace RecordCode
{
	public class FeaturedGameParticipant : PlayerStats
	{
		protected int _championId;
		protected int _skinIndex;
		protected int _profileIconId;
		public uint teamId
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
		public int spell1Id
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
		public int spell2Id
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
		public int championId
		{
			get
			{
				return this._championId;
			}
			set
			{
				this._championId = value;
			}
		}
		public int skinIndex
		{
			get
			{
				return this._skinIndex;
			}
			set
			{
				this._skinIndex = value;
			}
		}
		public int profileIconId
		{
			get
			{
				return this._profileIconId;
			}
			set
			{
				this._profileIconId = value;
			}
		}
		public string summonerName
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
		public bool bot
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
		public string QFSearchString
		{
			get
			{
				return string.Format("QFsearch", base.SummonerName);
			}
		}
	}
}
