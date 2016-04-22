using FluorineFx;
using FluorineFx.AMF3;
using System;
using System.Collections.Generic;
namespace RecordCode
{
    public class DetailStats
    {
        private System.Collections.Generic.Dictionary<string, uint> _statsCollection;
        private uint[] _itemsArray;
        public uint K
        {
            get
            {
                return this._statsCollection["CHAMPIONS_KILLED"];
            }
        }
        public uint D
        {
            get
            {
                return this._statsCollection["NUM_DEATHS"];
            }
        }
        public uint A
        {
            get
            {
                return this._statsCollection["ASSISTS"];
            }
        }
        public uint Level
        {
            get
            {
                return this._statsCollection["LEVEL"];
            }
        }
        public uint MinionsKilled
        {
            get
            {
                return this._statsCollection["MINIONS_KILLED"];
            }
        }
        public uint GoldEarned
        {
            get
            {
                return this._statsCollection["GOLD_EARNED"];
            }
        }
        public string KGoldEarned
        {
            get
            {
                double num = this._statsCollection["GOLD_EARNED"];
                num /= 1000.0;
                return string.Format("${0:.0}K", num);
            }
        }
        public string KDA
        {
            get
            {
                return string.Format("{0,-2}/{1,-2}/{2,-2}", this.K, this.D, this.A);
            }
        }
        public double KDAValue
        {
            get
            {
                if (this.D == 0)
                {
                    return this.K + this.A;
                }
                return (this.K + this.A) / this.D;
            }
        }
        public bool Win
        {
            get
            {
                return this._statsCollection.ContainsKey("WIN");
            }
        }
        public uint Item0
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[0];
            }
        }
        public uint Item1
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[1];
            }
        }
        public uint Item2
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[2];
            }
        }
        public uint Item3
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[3];
            }
        }
        public uint Item4
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[4];
            }
        }
        public uint Item5
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[5];
            }
        }
        public uint Item6
        {
            get
            {
                if (this._itemsArray == null)
                {
                    this.InitItemArray();
                }
                return this._itemsArray[6];
            }
        }
        private void InitItemArray()
        {
            this._itemsArray = new uint[7];
            string arg = "ITEM";
            int num = 0;
            for (int i = 0; i <= 6; i++)
            {
                uint num2;
                if (this._statsCollection.ContainsKey(arg + i))
                {
                    num2 = this._statsCollection[arg + i];
                }
                else
                {
                    num2 = 0u;
                }
                if (num2 != 0u)
                {
                    this._itemsArray[num++] = this._statsCollection[arg + i];
                }
            }
        }
        public DetailStats(ArrayCollection dStats)
        {
            this._statsCollection = new System.Collections.Generic.Dictionary<string, uint>();
            foreach (ASObject aSObject in dStats)
            {
                this._statsCollection.Add(aSObject["statTypeName"] as string, uint.Parse(aSObject["value"].ToString()));
            }
        }
    }
}
