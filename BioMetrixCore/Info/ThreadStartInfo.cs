using System.Threading;

namespace BioMetrixCore.Info
{
    public class ThreadStartInfo
    {
        public Thread Thread { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public ThreadStartInfo(Thread thread, CancellationTokenSource cancellationTokenSource)
        {
            Thread = thread;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
}
