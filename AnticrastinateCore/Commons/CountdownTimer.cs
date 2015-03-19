using System;
using System.Timers;

namespace AnticrastinateCore.Commons
{
    class CountdownTimer
    {
        private readonly Timer timer;

        private DateTime startTime;
        private readonly double totalDuration;

        public event EventHandler<ElapsedEventArgs> Elapsed;

        public CountdownTimer(double duration)
        {
            totalDuration = duration;
            timer = new Timer(duration) { AutoReset = false };
            timer.Elapsed += OnElapsed;
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            // reset the timer interval, in case we paused it.
            timer.Interval = totalDuration;
            Elapsed.Raise(sender, e);
        }

        public void Start()
        {
            startTime = DateTime.Now;
            timer.Start();
        }

        public void Pause()
        {
            timer.Stop();
            timer.Interval = TimeRemaining;
        }

        public double TimeRemaining
        {
            get
            {
                if (timer.Enabled)
                    return timer.Interval - (DateTime.Now - startTime).TotalMilliseconds;
                else
                    return timer.Interval;
            }
        }


    }
}
