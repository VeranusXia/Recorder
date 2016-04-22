
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
namespace RecordCode
{
	public class FeaturedGameJson : INotifyPropertyChanged
	{
		public ulong gameId;
		public int mapId;
		public string gameMode;
		public string gameType;
		public int gameQueueConfigId;
		public FeaturedGameParticipant[] participants;
		private FeaturedGameParticipant[] _purpleTeamParticipants;
		private FeaturedGameParticipant[] _blueTeamParticipants;
		public JContainer observers;
		public string platformId;
		public int gameTypeConfigId;
		public object bannedChampions;
		public ulong gameStartTime;
		public uint gameLength;
		public event PropertyChangedEventHandler PropertyChanged;
		public int MapId
		{
			get
			{
				return this.mapId;
			}
		}
		public string GameMode
		{
			get
			{
				return this.gameMode;
			}
		}
		public string GameType
		{
			get
			{
				return this.gameType;
			}
		}
		public FeaturedGameParticipant[] PurpleTeamParticipants
		{
			get
			{
				if (this._purpleTeamParticipants == null)
				{
					this.CreateTeamArray();
				}
				return this._purpleTeamParticipants;
			}
		}
		public FeaturedGameParticipant[] BlueTeamParticipants
		{
			get
			{
				if (this._blueTeamParticipants == null)
				{
					this.CreateTeamArray();
				}
				return this._blueTeamParticipants;
			}
		}
		public string PlatformId
		{
			get
			{
				return this.platformId;
			}
		}
		public uint GameLength
		{
			get
			{
				System.DateTime d = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
				d = d.AddMilliseconds(this.gameStartTime);
				return (uint)(System.DateTime.UtcNow - d).TotalSeconds - 180u;
			}
			set
			{
				this.gameLength = value;
				this.NotifyPropertyChanged("GameLength");
			}
		}
		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		private void CreateTeamArray()
		{
			this._blueTeamParticipants = new FeaturedGameParticipant[5];
			this._purpleTeamParticipants = new FeaturedGameParticipant[5];
			int num = 0;
			int num2 = 0;
			FeaturedGameParticipant[] array = this.participants;
			for (int i = 0; i < array.Length; i++)
			{
				FeaturedGameParticipant featuredGameParticipant = array[i];
				if (featuredGameParticipant.TeamId == 100u)
				{
					this._blueTeamParticipants[num++] = featuredGameParticipant;
				}
				else
				{
					this._purpleTeamParticipants[num2++] = featuredGameParticipant;
				}
			}
		}
	}
}
