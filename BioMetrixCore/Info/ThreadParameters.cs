using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace BioMetrixCore.Info
{
    public class ThreadParameters
    {
        public Thread Thread { get; set; }  
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Machine Machine { get; set; }    
        public int Index { get; set; }
        public Timer Timer { get; set; }
        public Timer CountingTimer { get; set; }
        public ThreadParameters(Thread thread, CancellationTokenSource cancellationTokenSource, Machine machine, int index, Timer timer, Timer countingTimer)
        {
            Thread = thread;
            CancellationTokenSource = cancellationTokenSource;
            CancellationToken = cancellationTokenSource.Token;
            Machine = machine;
            Index = index;
            Timer = timer;
            CountingTimer = countingTimer;
        }
    }
}
