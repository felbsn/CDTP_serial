using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CDTP_serial
{
    class TimeSimulator
    {
        DatePicker targetDate;
        DispatcherTimer dispatcherTimer;
        int intervalSeconds = 1;
        DateTime current;

        public int IntervalSecs { get => intervalSeconds; set { intervalSeconds = value; dispatcherTimer.Interval = new TimeSpan(0, 0, 0, intervalSeconds, 0); } }

        public event Action Tick;

        public DateTime Today { get
            {
                return current;
            }
        }

        public TimeSimulator(DatePicker targetDate)
        {
            this.targetDate = targetDate;
            dispatcherTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, intervalSeconds, 0)
            };

            dispatcherTimer.Tick += (s, e) =>
            {
                Tick?.Invoke();
                current = targetDate.SelectedDate.Value.AddDays(1);
                targetDate.SelectedDate = current;
          
            };
        }
        public void Start()
        {
            dispatcherTimer.Start();
        }
        public void Stop()
        {
            dispatcherTimer.Stop();
        }
        public bool isEnabled { get=> dispatcherTimer.IsEnabled; }
    }
}
