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
        Calendar calendar;
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

        public TimeSimulator(DatePicker targetDate ,Calendar calendar = null)
        {
            this.targetDate = targetDate;
            dispatcherTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, intervalSeconds, 0)
            };

            this.calendar = calendar;
            dispatcherTimer.Tick += (s, e) =>
            {
                current = targetDate.SelectedDate.Value.AddDays(1);
                targetDate.SelectedDate = current;
                if (this.calendar != null)
                {
                    this.calendar.SelectionMode = CalendarSelectionMode.SingleDate;
                    this.calendar.DisplayDate = current;
                    this.calendar.SelectedDate = current;

                }

                Tick?.Invoke();

                
            };

          
        }
        public void Start()
        {
            dispatcherTimer.Start();
            targetDate.IsEnabled = false;
            
        }
        public void Stop()
        {
            dispatcherTimer.Stop();
            targetDate.IsEnabled = true;
        }
        public bool isEnabled { get=> dispatcherTimer.IsEnabled; }
    }
}
