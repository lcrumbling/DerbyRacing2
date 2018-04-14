using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SQLite;

namespace GSRacing.RacingObjects
{
    public class RaceEvent : ObservableObject
    {
        private Guid _eventID;
        [PrimaryKey]
        public Guid EventID
        {
            get { return _eventID; }
            set
            {
                this.Set(ref this._eventID, value);
            }
        }

        private string _eventName;
        public string EventName
        {
            get { return _eventName; }
            set
            {
                this.Set(ref this._eventName, value);
            }
        }

        private DateTime _eventDate;
        [Indexed]
        public DateTime EventDate
        {
            get { return _eventDate; }
            set
            {
                this.Set(ref this._eventDate, value);
            }
        }

        private ObservableCollection<Racer> _racers;
        [Ignore]
        public ObservableCollection<Racer> Racers
        {
            get { return _racers; }
            set
            {
                this.Set(ref this._racers, value);
            }
        }

        private ObservableCollection<RaceHeat> _heats;
        [Ignore]
        public ObservableCollection<RaceHeat> Heats
        {
            get { return _heats; }
            set
            {
                this.Set(ref this._heats, value);
            }
        }

        private ObservableCollection<RaceResult> _results;
        [Ignore]
        public ObservableCollection<RaceResult> Results
        {
            get { return _results; }
            set
            {
                this.Set(ref this._results, value);
            }
        }

        private int _trackCount = 4;
        public int TrackCount
        {
            get { return _trackCount; }
            set
            {
                this.Set(ref this._trackCount, value);
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

        [Ignore]
        public IEnumerable<HeatTime> AllHeats
        {
            get
            {
                var allHeats = from t in Heats.
                            SelectMany(l => l.HeatTimes)
                               select t;
                return allHeats;
            }

        }

        public RaceEvent()
        {
            this.Racers = new ObservableCollection<Racer>();
            this.Heats = new ObservableCollection<RaceHeat>();
            this.Results = new ObservableCollection<RaceResult>();
        }

        public RaceEvent(Guid guidEventID, SQLiteConnection db)
        {
            this.Load(guidEventID, db);
        }

        public void Load(Guid guidEventID, SQLiteConnection db)
        {
            RaceEvent re = db.Table<RaceEvent>().Where(x => x.EventID == guidEventID).First();
            this.Completed = re.Completed;
            this.EventDate = re.EventDate;
            this.EventID = re.EventID;
            this.EventName = re.EventName;
            this.TrackCount = re.TrackCount;

            List<Racer> liRacer = db.Table<Racer>().Where(x => x.EventID == guidEventID).ToList();
            this.Racers = new ObservableCollection<Racer>(liRacer);

            List<RaceHeat> liHeats = db.Table<RaceHeat>().Where(x => x.EventID == guidEventID).ToList();
            this.Heats = new ObservableCollection<RaceHeat>(liHeats);

            foreach (RaceHeat rh in this.Heats)
            {
                List<HeatTime> liHeatTime = db.Table<HeatTime>().Where(x => x.HeatID == rh.HeatID).ToList();
                rh.HeatTimes = new ObservableCollection<HeatTime>(liHeatTime);
                foreach (HeatTime currHT in rh.HeatTimes)
                {
                    currHT.ParentEvent = this;
                    currHT.RacerID = currHT.RacerID;
                }
            }

            List<RaceResult> liResults = db.Table<RaceResult>().Where(x => x.EventID == guidEventID).ToList();
            this.Results = new ObservableCollection<RaceResult>(liResults);
            foreach (RaceResult currRR in this.Results)
            {
                currRR.ParentEvent = this;
                currRR.RacerID = currRR.RacerID;
            }

        }

        public Racer GetRacer(Guid racerGuid)
        {
            Racer r = this.Racers.First(x => x.RacerID == racerGuid);
            return r;
        }

        public void CreateHeats(SQLiteConnection db)
        {
            // each race must run on each lane once.
            // x lanes are available to race on simultaneously
            // heatcount = racers * x / x = racers
            int HeatCount = this.Racers.Count;
            int iTracks = this.TrackCount;

            //generate shuffled list of racers
            Random rng = new Random();
            List<Racer> shuffledRacers = this.Racers.Shuffle(rng).ToList();

            // create the heats + heat times
            for (int i = 0; i < HeatCount; i++)
            {
                RaceHeat h = new RaceHeat();
                h.EventID = this.EventID;
                h.HeatID = Guid.NewGuid();
                h.HeatNumber = (i + 1);
                this.Heats.Add(h);

                for (int j = 0; j < iTracks; j++)
                {
                    HeatTime ht = new HeatTime(this);
                    ht.HeatTimeID = Guid.NewGuid();
                    ht.HeatID = h.HeatID;
                    ht.TrackNumber = j + 1;
                    ht.RaceTime = null;
                    ht.RacerID = null;
                    h.HeatTimes.Add(ht);
                }
            }

            for (int i = 0; i < iTracks; i++)
            {
                using (IEnumerator<Racer> ienumShuffled = shuffledRacers.GetEnumerator())
                {
                    foreach (RaceHeat heat in this.Heats)
                    {
                        if (!ienumShuffled.MoveNext())
                            break;
                        HeatTime ht = heat.HeatTimes.First(x => x.TrackNumber == (i + 1));
                        ht.RacerID = ienumShuffled.Current.RacerID;
                    }
                }
                Racer[] rgShuffledRacers = shuffledRacers.ToArray();
                int RotateBy = HeatCount / iTracks;

                // if less tracks than racers, we'll run into an issue where we won't rotate
                if (RotateBy == 0)
                    RotateBy = 1;

                rgShuffledRacers.RotateLeft(RotateBy);
                shuffledRacers = new List<Racer>(rgShuffledRacers);
            }

#if DEBUG
            StringBuilder sb = new StringBuilder();
            foreach (RaceHeat rh in this.Heats)
            {
                sb.AppendFormat("Heat {0}:  ", rh.HeatNumber.ToString().PadLeft(2));
                foreach (HeatTime ht in rh.HeatTimes)
                {
                    sb.AppendFormat("({0}): {1} {2}. ", ht.TrackNumber, ht.Racer.FirstName, ht.Racer.LastName.Substring(0, 1));
                }
                sb.AppendLine();
            }
            //System.Diagnostics.Debug.Print(sb.ToString());
#endif

        }

        public void CalculateResults(SQLiteConnection db)
        {
            IEnumerable<HeatTime> allHeats = this.AllHeats;
            foreach (Racer r in this.Racers)
            {
                RaceResult res = new RaceResult(this);
                res.RaceResultID = Guid.NewGuid();
                res.EventID = this.EventID;
                res.RacerID = r.RacerID;
                res.AvgRaceTime = allHeats.Where(hi => hi.RacerID == r.RacerID).Average(x => x.RaceTime.Value);
                this.Results.Add(res);
            }

            IOrderedEnumerable<RaceResult> ioerr = this.Results.OrderBy(x => x.AvgRaceTime);
            int i = 1;
            foreach (RaceResult rr in ioerr)
            {
                rr.PlaceNumber = i;
                i++;
            }
            this.Results = new ObservableCollection<RaceResult>(ioerr);
        }

        public void Save(SQLiteConnection db)
        {
            RaceEvent re = db.Find<RaceEvent>(x => x.EventID == this.EventID);
            if (re == null)
                db.Insert(this);
            else
                db.Update(this);

            foreach (Racer r in this.Racers)
            {
                r.Save(db);
            }

            foreach (RaceHeat rh in this.Heats)
            {
                rh.Save(db);
            }

            foreach (RaceResult rr in this.Results)
            {
                rr.Save(db);
            }

        }

        public static void Delete(Guid EventID, SQLiteConnection db)
        {
            db.Delete<RaceEvent>(EventID);
        }

        public static ObservableCollection<RaceEvent> AllEvents(SQLiteConnection db)
        {
            var query = db.Table<RaceEvent>();
            return new ObservableCollection<RaceEvent>(query.ToList());
        }
    }
}
