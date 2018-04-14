using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;

namespace TimerMain
{
    public class FinishGate : IDisposable
    {
        public int TrackNumber { get; set; }
        public bool HasFinished { get; set; }
        public decimal TrackTime { get; set; }
        GpioPin FinishGatePin = null;
        private System.Diagnostics.Stopwatch HeatStopWatch = null;

        public FinishGate(int _TrackNumber, GpioController gpio, int GatePinNumber, System.Diagnostics.Stopwatch sw)
        {
            this.TrackNumber = _TrackNumber;
            FinishGatePin = gpio.OpenPin(GatePinNumber);
            FinishGatePin.SetDriveMode(GpioPinDriveMode.Input);
            FinishGatePin.ValueChanged += FinishGatePin_ValueChanged;
            HeatStopWatch = sw;
        }

        public event EventHandler<TrackCompletedEventArgs> TrackCompleted;

        protected virtual void OnTrackCompleted(TrackCompletedEventArgs e)
        {
            EventHandler<TrackCompletedEventArgs> handler = TrackCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private async void FinishGatePin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Track " + this.TrackNumber + " Value: " + args.Edge);
            if (args.Edge == GpioPinEdge.RisingEdge)
                return;

            if (!ViewModels.MainViewModel.Instance.InRace)
                return;

            if (this.HasFinished)
                return;

            this.HasFinished = true;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.TrackTime = ((decimal)HeatStopWatch.ElapsedMilliseconds) / 1000M;
                TrackCompletedEventArgs e = new TrackCompletedEventArgs();
                e.ElapsedTime = TrackTime;
                e.TrackNo = this.TrackNumber;
                OnTrackCompleted(e);
            });
        }

        public void ResetTrack()
        {
            this.HasFinished = false;
            this.TrackTime = 0M;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.FinishGatePin.ValueChanged -= FinishGatePin_ValueChanged;
                    this.FinishGatePin.Dispose();
                    this.FinishGatePin = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TrackInfo() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
    public class TrackCompletedEventArgs : EventArgs
    {
        public decimal ElapsedTime { get; set; }
        public int TrackNo { get; set; }
    }
}
