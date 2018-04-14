using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SQLite;

namespace GSRacing.RacingObjects
{
    public class RaceHeat: ObservableObject
    {
        private Guid _heatID;
        [PrimaryKey]
        public Guid HeatID
        {
            get { return _heatID; }
            set
            {
                this.Set(ref this._heatID, value);
            }
        }

        private int _heatNumber;
        [Indexed]
        public int HeatNumber
        {
            get { return _heatNumber; }
            set
            {
                this.Set(ref this._heatNumber, value);
            }
        }

        private Guid _eventID;
        [Indexed]
        public Guid EventID
        {
            get { return _eventID; }
            set
            {
                this.Set(ref this._eventID, value);
            }
        }

        private ObservableCollection<HeatTime> _heatTimes;
        [Ignore]
        public ObservableCollection<HeatTime> HeatTimes
        {
            get { return _heatTimes; }
            set
            {
                this.Set(ref this._heatTimes, value);
            }
        }
        private bool _completed = false;
        public bool Completed
        {
            get { return _completed; }
            set
            {
                this.Set(ref this._completed, value);
            }
        }

        public RaceHeat()
        {
            HeatTimes = new ObservableCollection<HeatTime>();
        }

        public bool HasRacer(Guid RacerID)
        {
            // is this racer already in this heat?
            return (this.HeatTimes.Count(x => x.RacerID == RacerID) > 0);
        }

        public void ForceComplete(decimal ForceCompletedTime)
        {
            foreach(HeatTime ht in this.HeatTimes)
            {
                if (!ht.RaceTime.HasValue)
                {
                    ht.RaceTime = ForceCompletedTime;
                }
            }
            this.Completed = true;
        }

        public void Save(SQLiteConnection db)
        {
            RaceHeat re = db.Find<RaceHeat>(x => x.HeatID == this.HeatID);
            if (re == null)
                db.Insert(this);
            else
                db.Update(this);

            foreach (HeatTime ht in this.HeatTimes)
            {
                ht.Save(db);
            }
        }

    }


}
