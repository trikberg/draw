using System;
using System.Timers;

namespace Draw.Client.Services
{
    public class TurnTimer : IDisposable
    {
        public event EventHandler<int>? TurnTimerChanged;
        public int RemainingSeconds { get; private set; } = 0;
        private Timer timer;

        internal TurnTimer()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += TimerElapsed;
        }

        public void Dispose()
        {
            timer.Dispose();
        }

        internal void StartTimer(int time)
        {
            RemainingSeconds = time;
            timer.Start();
            TurnTimerChanged?.Invoke(this, RemainingSeconds);
        }

        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            RemainingSeconds -= 1;
            TurnTimerChanged?.Invoke(this, RemainingSeconds);
            if (RemainingSeconds <= 0)
            {
                timer.Stop();
            }
        }

        internal void Stop()
        {
            timer.Stop();
        }

        internal void SetTime(int timeRemaining)
        {
            RemainingSeconds = timeRemaining;
        }
    }
}
