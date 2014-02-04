using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prism.General
{
    public delegate void PollManagerCompleteHandler();
    public delegate void PollManagerTerminateHandler();
    public delegate void PollManagerActionHandler(PollManagerCompleteHandler complete);
    public delegate void PollManagerPauseEventHandler(object sender);
    public delegate void PollManagerResumeEventHandler(object sender);

    public interface IPollManager
    {
        event PollManagerPauseEventHandler PollManagerPauseEvent;
        event PollManagerResumeEventHandler PollManagerResumeEvent;

        void EnqueuePoll(IUnit unit, PollManagerActionHandler action, PollManagerTerminateHandler terminate);
        void Silent(TimeSpan time, PollManagerCompleteHandler compelete);
    }
}
