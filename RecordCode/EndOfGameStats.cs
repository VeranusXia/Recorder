using FluorineFx;
using FluorineFx.AMF3;
using FluorineFx.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
namespace RecordCode
{
	public class EndOfGameStats : INotifyPropertyChanged
	{
		private byte[] _rawData;
		private bool _broken;
		private bool _ranked;
		private string _gameType;
		private ulong _gameId;
		private uint _gameLength;
		private string _gameMode;
		private System.Collections.Generic.List<PlayerStats> _players;
		private string _blueTeamInfo;
		private string _purpleTeamInfo;
		private bool _decoded;
		public event PropertyChangedEventHandler PropertyChanged;
		public bool Broken
		{
			get
			{
				return this._broken;
			}
		}
		public bool Ranked
		{
			get
			{
				this.DecodeData();
				return this._ranked;
			}
			set
			{
				this._ranked = value;
				this.NotifyPropertyChanged("Ranked");
			}
		}
		public string GameType
		{
			get
			{
				this.DecodeData();
				return this._gameType;
			}
			set
			{
				this._gameType = value;
				this.NotifyPropertyChanged("GameType");
			}
		}
		public ulong GameId
		{
			get
			{
				this.DecodeData();
				return this._gameId;
			}
			set
			{
				this._gameId = value;
				this.NotifyPropertyChanged("GameId");
			}
		}
		public uint GameLength
		{
			get
			{
				this.DecodeData();
				return this._gameLength;
			}
			set
			{
				this._gameLength = value;
				this.NotifyPropertyChanged("GameLength");
			}
		}
		public string GameMode
		{
			get
			{
				this.DecodeData();
				return this._gameMode;
			}
			set
			{
				this._gameMode = value;
				this.NotifyPropertyChanged("GameMode");
			}
		}
		public System.Collections.Generic.List<PlayerStats> Players
		{
			get
			{
				this.DecodeData();
				return this._players;
			}
			set
			{
				this._players = value;
				this.NotifyPropertyChanged("Players");
			}
		}
		public string BlueTeamKDA
		{
			get
			{
				this.DecodeData();
				uint num = 0u;
				uint num2 = 0u;
				uint num3 = 0u;
				foreach (PlayerStats current in this.Players)
				{
					if (current.TeamId == 100u)
					{
						num += current.DetailStats.K;
						num2 += current.DetailStats.D;
						num3 += current.DetailStats.A;
					}
				}
				return string.Format("{0,-2}/{1,-2}/{2,-2}", num, num2, num3);
			}
		}
		public string WonTeam
		{
			get
			{
				if (this.Players.Count == 0)
				{
					return "NoResult";
				}
				if ((this.Players[0].DetailStats.Win && this.Players[0].TeamId == 100u) || (!this.Players[0].DetailStats.Win && this.Players[0].TeamId == 200u))
				{
					return "BlueTeamWon";
				}
				return "PurpleTeamWon";
			}
		}
		public string PurpleTeamKDA
		{
			get
			{
				this.DecodeData();
				uint num = 0u;
				uint num2 = 0u;
				uint num3 = 0u;
				foreach (PlayerStats current in this.Players)
				{
					if (current.TeamId == 200u)
					{
						num += current.DetailStats.K;
						num2 += current.DetailStats.D;
						num3 += current.DetailStats.A;
					}
				}
				return string.Format("{0,-2}/{1,-2}/{2,-2}", num, num2, num3);
			}
		}
		public string BlueTeamInfo
		{
			get
			{
				this.DecodeData();
				return this._blueTeamInfo;
			}
			set
			{
				this._blueTeamInfo = value;
				this.NotifyPropertyChanged("BlueTeamInfo");
			}
		}
		public string PurpleTeamInfo
		{
			get
			{
				this.DecodeData();
				return this._purpleTeamInfo;
			}
			set
			{
				this._purpleTeamInfo = value;
				this.NotifyPropertyChanged("PurpleTeamInfo");
			}
		}
		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		public bool DecodeData()
		{
			if (this._decoded)
			{
				return !this.Broken;
			}
			this._decoded = true;
			System.IO.Stream stream = new System.IO.MemoryStream(this._rawData);
			using (AMFReader aMFReader = new AMFReader(stream))
			{
				try
				{
					ASObject aSObject = (ASObject)aMFReader.ReadAMF3Data();
					this.Ranked = (bool)aSObject["ranked"];
					this.GameType = (aSObject["gameType"] as string);
					this.GameLength = uint.Parse(aSObject["gameLength"].ToString());
					this.GameMode = (aSObject["gameMode"] as string);
					this.GameId = ulong.Parse(aSObject["gameId"].ToString());
					ArrayCollection arrayCollection = aSObject["teamPlayerParticipantStats"] as ArrayCollection;
					ArrayCollection arrayCollection2 = aSObject["otherTeamPlayerParticipantStats"] as ArrayCollection;
					int arg_DB_0 = arrayCollection.Count;
					int arg_E3_0 = arrayCollection2.Count;
					this.Players = new System.Collections.Generic.List<PlayerStats>();
					for (int i = 0; i < arrayCollection.Count; i++)
					{
						this.Players.Add(new PlayerStats(arrayCollection[i] as ASObject));
					}
					for (int j = 0; j < arrayCollection2.Count; j++)
					{
						this.Players.Add(new PlayerStats(arrayCollection2[j] as ASObject));
					}
					if (aSObject["myTeamInfo"] != null)
					{
						ASObject aSObject2 = aSObject["myTeamInfo"] as ASObject;
						this.BlueTeamInfo = string.Format("[{0}]{1}", aSObject2["tag"], aSObject2["name"]);
						aSObject2 = (aSObject["otherTeamInfo"] as ASObject);
						this.PurpleTeamInfo = string.Format("[{0}]{1}", aSObject2["tag"], aSObject2["name"]);
					}
				}
				catch (System.Exception)
				{
					this._broken = true;
					return false;
				}
			}
			return true;
		}
		public EndOfGameStats()
		{
			this._broken = false;
		}
		public EndOfGameStats(byte[] statBytes)
		{
			this._broken = false;
			this._rawData = statBytes;
		}
	}
}
