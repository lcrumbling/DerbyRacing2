using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SQLite;

namespace GSRacing.RacingObjects
{
    public class HeatTime : ObservableObject
    {
        private Guid _heatTimeID;
        [PrimaryKey]
        public Guid HeatTimeID
        {
            get { return _heatTimeID; }
            set
            {
                this.Set(ref this._heatTimeID, value);
            }
        }

        private Guid _heatID;
        [Indexed]
        public Guid HeatID
        {
            get { return _heatID; }
            set
            {
                this.Set(ref this._heatID, value);
            }
        }

        private Guid? _racerID;
        public Guid? RacerID
        {
            get { return _racerID; }
            set
            {
                this.Set(ref this._racerID, value);
                if (ParentEvent != null)
                {
                    this.Racer = value.HasValue ? ParentEvent.GetRacer(value.Value) : null;
                    RaisePropertyChanged("Racer");
                }
            }
        }

        private int _trackNumber;
        [Indexed]
        public int TrackNumber
        {
            get { return _trackNumber; }
            set
            {
                this.Set(ref this._trackNumber, value);
            }
        }

        private decimal? _raceTime;
        public decimal? RaceTime
        {
            get { return _raceTime; }
            set
            {
                this.Set(ref this._raceTime, value);
                RaisePropertyChanged("FormattedRaceTime");
            }
        }

        private Racer _racer;
        [Ignore]
        public Racer Racer
        {
            get { return _racer; }
            set
            {
                this.Set(ref this._racer, value);
            }
        }

        private RaceEvent _parentEvent;
        [Ignore]
        public RaceEvent ParentEvent 
        {
            get
            {
                return _parentEvent;
            }
            set
            {
                this.Set(ref _parentEvent, value);
            }
        }

        [Ignore]
        public string FormattedRaceTime
        {
            get
            {
                if (this.RaceTime.HasValue)
                    return string.Format("{0}s", this.RaceTime.Value);

                return "---";
            }
        }

        /// <summary>
        /// This public, parameterless constructor is only here for
        /// SQLiteConnection.Find() and should not otherwise be used.
        /// </summary>
        public HeatTime()
        {

        }
        public HeatTime(RaceEvent raceEvent)
        {
            this._parentEvent = raceEvent;
        }

        public void Save(SQLiteConnection db)
        {
            HeatTime re = db.Find<HeatTime>(x => x.HeatTimeID == this.HeatTimeID);
            if (re == null)
                db.Insert(this);
            else
                db.Update(this);
        }

    }
}
