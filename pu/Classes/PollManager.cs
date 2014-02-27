using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using Prism.General;

namespace Prism.Classes
{    
    public delegate void PollManagerChangeStateEventHandler(bool isBusy);
    public delegate void PollManagerUpdateUnitStateEventHandler(IUnit unit);

    public class PollManager : IPollManager
    {
        public static PollManager Instance = null;

        private class PollItem
        {
            public IUnit Unit;
            public PollManagerActionHandler Action;
            public PollManagerTerminateHandler Terminate;

            public PollItem(IUnit unit, PollManagerActionHandler action, PollManagerTerminateHandler terminate)
            {
                Unit = unit;
                Action = action;
                Terminate = terminate;
            }
        }

        public class SilentTimer
        {           
            public delegate void SilentTimerTimeElapsedEventHandler(SilentTimer timer);
            public PollManagerCompleteHandler Complete { get; set; }

            public SilentTimer(TimeSpan time, PollManagerCompleteHandler complete, SilentTimerTimeElapsedEventHandler elapsed)
            {
                Complete = complete;

                System.Timers.Timer silentTimer = new System.Timers.Timer(time.TotalMilliseconds);
                silentTimer.Elapsed += delegate(object sender, ElapsedEventArgs e)
                {
                    silentTimer.Stop();
                    elapsed(this);
                };
                silentTimer.Start();
            }
        }

        public event PollManagerChangeStateEventHandler PollManagerChangeStateEvent;
        public event PollManagerUpdateUnitStateEventHandler PollManagerUpdateUnitStateEvent;

        public event PollManagerPauseEventHandler PollManagerPauseEvent;
        public event PollManagerResumeEventHandler PollManagerResumeEvent;

        private Queue<PollItem> PollQueue;
        private ManualResetEvent ResumePollEvent;
        private Thread PollThread;
        private bool lastBusyState = false;
        
        private PollItem CurrentPollItem = null;
        private ManualResetEvent DonePollEvent = null;
        private System.Timers.Timer DonePollTimer = null;        
        private List<SilentTimer> SilentTimers = null;
        
        public PollManager()
        {
            SilentTimers = new List<SilentTimer>();

            PollQueue = new Queue<PollItem>();
            ResumePollEvent = new ManualResetEvent(false);
            PollThread = new Thread(delegate()
            {
                try
                {
                    while (PollThread.IsAlive)
                    {
                        if (PollQueue.Count == 0 || SilentTimers.Count > 0)
                        {
                            if (lastBusyState == true && PollManagerChangeStateEvent != null)
                            {
                                PollManagerChangeStateEvent(false);
                            }

                            lastBusyState = false;
                            ResumePollEvent.Reset();
                            ResumePollEvent.WaitOne();
                        }
                        else
                        {
                            if (lastBusyState == false && PollManagerChangeStateEvent != null)
                            {
                                PollManagerChangeStateEvent(true);
                            }

                            lastBusyState = true;
                            CurrentPollItem = PollQueue.Dequeue();

                            if (CurrentPollItem != null)
                            {
                                DonePollEvent = new ManualResetEvent(false);
                                DonePollTimer = new System.Timers.Timer(90000);

                                DonePollTimer.Elapsed += delegate(object s, ElapsedEventArgs ev)
                                {
                                    DonePollTimer.Stop();                                   
                                    DonePollEvent.Set();
                                    CurrentPollItem.Terminate();
                                };

                                DonePollTimer.Start();
                                CurrentPollItem.Action(delegate()
                                {
                                    if (DonePollTimer != null)
                                    {
                                        DonePollTimer.Stop();
                                    }

                                    if (DonePollEvent != null)
                                    {
                                        DonePollEvent.Set();
                                    }
                                });

                                DonePollEvent.WaitOne();

                                if (PollThread.ThreadState != ThreadState.AbortRequested && PollThread.ThreadState != ThreadState.Aborted && PollManagerUpdateUnitStateEvent != null)
                                {
                                    PollManagerUpdateUnitStateEvent(CurrentPollItem.Unit);
                                }
                            }
                        }
                    }
                }
                catch (ThreadAbortException e)
                {
                }
            });            
        }

        ~PollManager()
        {
            try
            {
                PollThread.Abort();

                if (CurrentPollItem != null)
                {
                    try
                    {
                        CurrentPollItem.Terminate();
                        DonePollTimer.Stop();
                        DonePollEvent.Set();
                    }
                    catch (Exception e)
                    {
                    }
                }

                ResumePollEvent.Set();
                PollThread.Join();
            }
            catch (Exception e)
            {
            }            
        }

        public void Start()
        {
            if (PollThread != null)
            {
                PollThread.Start();
            }
        }

        public void EnqueuePoll(IUnit unit, PollManagerActionHandler action, PollManagerTerminateHandler terminate)
        {
            PollQueue.Enqueue(new PollItem(unit, action, terminate));
            ResumePollEvent.Set();
        }

        public void Silent(TimeSpan time, PollManagerCompleteHandler compelete)
        {
            if (SilentTimers.Count == 0 && PollManagerPauseEvent != null)
            {
                PollManagerPauseEvent(this);
            }

            SilentTimers.Add(new SilentTimer(time, compelete, SilentTimerElapsed));
        }

        private void SilentTimerElapsed(SilentTimer timer)
        {
            SilentTimers.Remove(timer);

            if (SilentTimers.Count == 0)
            {
                ResumePollEvent.Set();

                if (PollManagerResumeEvent != null)
                {
                    PollManagerResumeEvent(this);
                }                
            }
            
            timer.Complete();
        }
    }
}
