using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SQLite;

namespace GSRacing.RacingObjects
{
    public class Racer : ObservableObject
    {

        public Racer()
        {
        }
        
        public Racer(Guid guidRacerID)
        {
            
        }

        public void Save(SQLiteConnection db)
        {
            Racer r = db.Find<Racer>(x => x.RacerID == this.RacerID);
            if (r == null)
                db.Insert(this);
            else
                db.Update(this);
        }

        private Guid _racerID;
        [PrimaryKey]
        public Guid RacerID
        {
            get { return _racerID; }
            set
            {
                this.Set(ref this._racerID, value);
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

        private int _regNumber;
        public int RegNumber
        {
            get { return _regNumber; }
            set
            {
                this.Set(ref this._regNumber, value);
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                this.Set(ref this._firstName, value);
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                this.Set(ref this._lastName, value);
            }
        }

        [Ignore]
        public string FullName
        {
            get { return string.Format("{0} {1} [{2}]", this.FirstName, this.LastName, this.RegNumber);  }
        }

        [Ignore]
        public string CarNumber
        {
            get { return string.Format("#{2} [{0} {1}]", this.FirstName, this.LastName, this.RegNumber); }
        }
        

    }
}
