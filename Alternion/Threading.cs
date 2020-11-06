using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Alternion
{
    class Threading
    {

        public static void CallToChildThread()
        {
            Console.WriteLine("Child thread starts");

            // the thread is paused for 5000 milliseconds
            int sleepfor = 5000;

            Console.WriteLine("Child Thread Paused for {0} seconds", sleepfor / 1000);
            Thread.Sleep(sleepfor);
            Console.WriteLine("Child thread resumes");
        }

        void Engage()
        {
            ThreadStart childref = new ThreadStart(CallToChildThread);

            Thread childThread = new Thread(childref);

            childThread.Start();

            Console.ReadKey();
        }
    }
}
