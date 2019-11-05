using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Core
{
    public class Timer
    {
        // Delegates
        public delegate void Tick(object sender, EventArgs ea);

        // Properties 
        public int Interval;
        public Tick OnTimerTick;

        // Private Data 
        Thread _timerThread;
        volatile bool _bStop;

        public Thread Start()
        {
            _bStop = false;
            _timerThread = new Thread(new ThreadStart(Run));
            _timerThread.IsBackground = true;
            _timerThread.Start();
            return _timerThread;
        }

        public void Run()
        {
            while (!_bStop)
            {
                // Sleep for the timer interval 
                //Thread.Sleep(Interval);
                SleepIntermittently(Interval);
                // Then fire the tick event 
                OnTimerTick(this, new EventArgs());
            }
        }

        public void Stop()
        {
            // Request the loop to stop
            _bStop = true;
        }

        public void SleepIntermittently(int totalTime)
        {
            int sleptTime = 0;
            int intermittentSleepIncrement = 10;

            while (!_bStop && sleptTime < totalTime)
            {
                Thread.Sleep(intermittentSleepIncrement);
                sleptTime += intermittentSleepIncrement;
            }
        } 
    }
}
