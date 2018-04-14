using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SQLite;

namespace GSRacing.RacingObjects
{
    public class RaceResult : ObservableObject
    {
        private Guid _raceResultID;
        [PrimaryKey]
        public Guid RaceResultID
        {
            get { return _raceResultID; }
            set
            {
                this.Set(ref this._raceResultID, value);
            }
        }

        private int _placeNumber;
        [Indexed]
        public int PlaceNumber
        {
            get { return _placeNumber; }
            set
            {
                this.Set(ref this._placeNumber, value);
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

        private decimal? _avgRaceTime;
        public decimal? AvgRaceTime
        {
            get { return _avgRaceTime; }
            set
            {
                this.Set(ref this._avgRaceTime, value);
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
                if (this.AvgRaceTime.HasValue)
                    return string.Format("{0}s", this.AvgRaceTime.Value.ToString("F3"));

                return "---";
            }
        }

        public RaceResult()
        {
        }

        public RaceResult(RaceEvent raceEvent)
        {
            this._parentEvent = raceEvent;
        }

        public void Save(SQLiteConnection db)
        {
            RaceResult rr = db.Find<RaceResult>(x => x.RaceResultID == this.RaceResultID);
            if (rr == null)
                db.Insert(this);
            else
                db.Update(this);
        }

    }
}
