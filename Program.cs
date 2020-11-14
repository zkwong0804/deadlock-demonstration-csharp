using System;
using System.Threading;

namespace Deadlock
{
    class Program
    {
        private const string MUTEX = "mutex";
        private const string HOLD_WAIT = "holdnwait";
        private const string NO_PREEMPTION = "nopreemption";
        private const string CIRCULAR = "circularwait";
        private const int TOTAL_WORKER = 10;
        private static Thread[] _workers;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                // print guide
                _printManual();
            }
            else
            {
                switch(args[0])
                {
                    case MUTEX:
                        // demo mutex deadlock
                        Mutex();
                        break;
                    case HOLD_WAIT:
                        //demo hold 'n wait deadlock
                        HoldAndWait();
                        break;
                    case NO_PREEMPTION:
                        // demo no preemption deadlock
                        NoPremption();
                        break;
                    case CIRCULAR:
                        // demo circular wait deadlock
                        CircularWait();
                        break;
                    case "manual":
                        _printManual();
                        break;
                    default:
                        Console.WriteLine("Invalid args! Please follow the manual!");
                        _printManual();
                        break;
                }
            }


            void _printManual()
            {
                Console.WriteLine("Welcome to Deadlock demonstration!");
                Console.WriteLine("\nUsage:");
                Console.WriteLine("\t./Deadlock <args>");
                Console.WriteLine();
                Console.WriteLine("Arguments:");
                Console.WriteLine($"{MUTEX}\t\t=> Perform Mutual Exclusion deadlock");
                Console.WriteLine($"{HOLD_WAIT}\t=> Perform Hold And Wait deadlock");
                Console.WriteLine($"{CIRCULAR}\t=> Perform Circular Wait deadlock");
                Console.WriteLine($"{NO_PREEMPTION}\t=> Perform No Preemption deadlock");
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static void Mutex()
        {
            _workers = new Thread[TOTAL_WORKER];
            object lock1 = new object();
            void _lockAction()
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine($"{name} are waiting to lock {nameof(lock1)}");
                lock(lock1)
                {
                    Console.WriteLine($"{name} has locked {nameof(lock1)}");
                    while (true) continue;
                }
                Console.WriteLine($"{name} has released {nameof(lock1)}");
            };

            for (int i=0; i<TOTAL_WORKER; i++)
            {
                _workers[i] = new Thread(_lockAction);
                _workers[i].IsBackground = false;
                _workers[i].Start();
            }
        }

        private static void CircularWait()
        {
            object lock1 = new object();
            object lock2 = new object();
            object lock3 = new object();


            Thread t1 = new Thread(() =>
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine($"{name} are waiting to lock {nameof(lock1)}");
                lock(lock1)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"{name} has locked the {nameof(lock1)}");
                    Console.WriteLine($"{name} are waiting to lock {nameof(lock2)}");
                    lock(lock2)
                    {
                        Console.WriteLine($"{name} has locked the {nameof(lock2)}");
                    }

                    Console.WriteLine($"{name} has released the {nameof(lock2)}");
                }
                Console.WriteLine($"{name} has released the {nameof(lock1)}");
            });


            Thread t2 = new Thread(() =>
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine($"{name} are waiting to lock {nameof(lock2)}");
                lock(lock2)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"{name} has locked the {nameof(lock2)}");
                    Console.WriteLine($"{name} are waiting to lock {nameof(lock3)}");
                    lock(lock3)
                    {
                        Console.WriteLine($"{name} has locked the {nameof(lock3)}");
                    }

                    Console.WriteLine($"{name} has released the {nameof(lock3)}");
                }
                Console.WriteLine($"{name} has released the {nameof(lock2)}");
            });
            

            Thread t3 = new Thread(() =>
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine($"{name} are waiting to lock {nameof(lock3)}");
                lock(lock3)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine($"{name} has locked the {nameof(lock3)}");
                    Console.WriteLine($"{name} are waiting to lock {nameof(lock1)}");
                    lock(lock1)
                    {
                        Console.WriteLine($"{name} has locked the {nameof(lock1)}");
                    }

                    Console.WriteLine($"{name} has released the {nameof(lock1)}");
                }
                Console.WriteLine($"{name} has released the {nameof(lock3)}");
            });

            t1.IsBackground = false;
            t2.IsBackground = false;
            t3.IsBackground = false;

            t1.Start();
            t2.Start();
            t3.Start();

        }

        private static void NoPremption()
        {
            object mylock = new object();
            _workers = new Thread[TOTAL_WORKER];
            for (int i=0; i<TOTAL_WORKER; i++)
            {
                _workers[i] = new Thread(_waitAndLock);
                _workers[i].Start();
            }

            foreach(var worker in _workers)
            {
                worker.Join();
            }

            
            void _waitAndLock()
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                Console.WriteLine($"{name} are waiting to lock the resources!");
                lock(mylock)
                {
                    Console.WriteLine($"{name} has locked the resources!");
                    while (true) continue;
                }

                Console.WriteLine($"{name} has released the resources!");
            }
        }

        private static void HoldAndWait()
        {
            object lock1 = new object();
            object lock2 = new object();

            Thread t1 = new Thread(() => 
            {
                string name = $"Worker {Thread.CurrentThread.ManagedThreadId}";
                string lock1_name = $"Lock {nameof(lock1)}";
                string lock2_name = $"Lock {nameof(lock2)}";
                Console.WriteLine($"{name} are waiting to {lock1_name}");
                lock(lock1)
                {
                    Console.WriteLine($"{name} has locked {lock1_name}");
                    Thread.Sleep(3000);
                    Console.WriteLine($"{name} are waiting to lock {lock2_name}");
                    lock(lock2)
                    {
                        Console.WriteLine($"{name} has locked {lock2_name}");
                    }
                    Console.WriteLine($"{name} has released {lock2_name}");
                }
                Console.WriteLine($"{name} has released {lock1_name}");

            });

            Thread t2 = new Thread(() =>
            {
                Console.WriteLine($"Worker {Thread.CurrentThread.ManagedThreadId}" +
                    $" are waiting to lock {nameof(lock1)}");
                lock(lock1)
                {

                }
            });

            t1.Start();
            t2.Start();

            Console.WriteLine($"Main thread are waiting to lock {nameof(lock2)}");
            lock(lock2)
            {
                Console.WriteLine($"Main thread has locked {nameof(lock2)}");
                Console.WriteLine("Main thread is going to lock " +
                    $"{nameof(lock2)} for a period of time!");
                while (true) continue;
            }

            Console.WriteLine($"Main thread has released {nameof(lock2)}");
        }
    }
}
