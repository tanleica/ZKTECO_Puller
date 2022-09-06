using System.Threading;
using Timer = System.Windows.Forms.Timer;

namespace BioMetrixCore.Info
{
    public class ThreadParameters
    {
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Machine Machine { get; set; }    
        public int Index { get; set; }
        public Timer Timer { get; set; }    
        public ThreadParameters(CancellationTokenSource cancellationTokenSource, Machine machine, int index, Timer timer)
        {
            CancellationTokenSource = cancellationTokenSource;
            CancellationToken = cancellationTokenSource.Token;
            Machine = machine;
            Index = index;
            Timer = timer;
        }
    }
}
