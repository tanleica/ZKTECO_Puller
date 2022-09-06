using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BioMetrixCore.Info
{
    public class Puller
    {
        public static void StaticMethod(object obj)
        {
            CancellationToken ct = (CancellationToken)obj;
            Console.WriteLine("Puller.StaticMethod is running on another thread.");

            // Simulate work that can be canceled.
            while (!ct.IsCancellationRequested)
            {
                Thread.Sleep(500);
                Console.WriteLine("Puller.StaticMethod is running on another thread.");
            }
            Console.WriteLine("The worker thread has been canceled!");
        }
    }
}
