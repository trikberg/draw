using System;
using System.Timers;

namespace Draw.Server.Game
{
    public class GameTimer : IDisposable
    {
        private Timer timer;
        private DateTime startTime;
        private DateTime nextTrigger;

        public GameTimer(double interval, ElapsedEventHandler handler)
        {
            timer = new Timer(interval);
            timer.AutoReset = false;
            timer.Elapsed += handler;
        }

        public double TimeElapsed => (DateTime.Now - startTime).TotalMilliseconds;
        public double TimeRemaining => (nextTrigger - DateTime.Now).TotalMilliseconds;

        public double Change(double multiplier)
        {
            timer.Stop();
            double newInterval = multiplier * TimeRemaining;
            timer.Interval = newInterval;
            Start();
            return TimeRemaining;
        }

        public double Reset(double interval)
        {
            timer.Stop();
            timer.Interval = interval;
            Start();
            return TimeRemaining;
        }

        public void Start()
        {
            startTime = DateTime.Now;
            nextTrigger = startTime.Add(TimeSpan.FromMilliseconds(timer.Interval));
            timer.Start();
        }

        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
