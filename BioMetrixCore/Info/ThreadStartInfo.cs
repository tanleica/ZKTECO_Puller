using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace BioMetrixCore.Info
{
    public class ThreadStartInfo
    {
        public int Index { get; set; }
        public Thread Thread { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        //public Timer ChildTimer { get; set; }
        public int? TimeCount { get; set; }

        public ThreadStartInfo(int index, Thread thread, CancellationTokenSource cancellationTokenSource, /* Timer timer, */ int? timeCount = 0)
        {
            Index = index;
            Thread = thread;
            CancellationTokenSource = cancellationTokenSource;
            //ChildTimer = timer;
            TimeCount = timeCount;
        }
    }
}
