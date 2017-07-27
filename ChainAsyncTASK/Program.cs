using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tool = ChainAsyncTASK.Program.ChainAsyncTASK;

namespace ChainAsyncTASK
{
    class Program
    {
        internal class ChainAsyncTASK
        {
            internal delegate Task RaiseEventDeledate(TimeSpan dellay, CancellationToken token);
            private static event RaiseEventDeledate _StartTasksEvent = null;
            private async Task CooldownEventHandlerAsync(TimeSpan dellay, CancellationToken token) {
                await Task.Run(async () => {
                    var timer = Stopwatch.StartNew();
                    Console.WriteLine($"\r\nFirst method start.Going to sleep {dellay.Seconds} sec");
                    await Task.Delay(dellay, token);
                    Console.WriteLine($"\r\nFirst method end. Worked {timer.Elapsed.Seconds} s");
                    await SecondMethodAsync(token);
                }, token);
            }

            internal int MinSleepSeconds { get; }
            internal int MaxSleepSeconds { get; }
            internal Random Random { get; }
            internal CancellationToken CancellationToken {get; private set; }

            internal ChainAsyncTASK(int disableSeconds, int minSleepSeconds, int maxSleepSeconds)
            {
                CancellationToken = new CancellationTokenSource(disableSeconds * 1000).Token;
                MinSleepSeconds = minSleepSeconds;
                MaxSleepSeconds = maxSleepSeconds;
                Random = new Random();
                _StartTasksEvent += CooldownEventHandlerAsync;
            }
            public async Task SecondMethodAsync(CancellationToken token) {
                var timer = Stopwatch.StartNew();
                var _sleep = Random.Next(MinSleepSeconds, MaxSleepSeconds);
                Console.WriteLine($"\r\nSecond method start. Going to sleep {_sleep} s");

                await Task.Delay(_sleep * 1000, token);
                Console.WriteLine($"\r\nSecond method end in {timer.Elapsed.Seconds} s");
                Console.WriteLine(new string('*', 30));
                Console.ForegroundColor = ConsoleColor.Cyan;
                var _cooldownTimeout = Random.Next(MinSleepSeconds, MaxSleepSeconds);
                Console.WriteLine($"\r\nCooldown...Waiting for {_cooldownTimeout} sec");
                await CallCooldownEventHandlerAsync(TimeSpan.FromSeconds(_cooldownTimeout), token, timer);
            }

            private async Task CallCooldownEventHandlerAsync(TimeSpan dellay, CancellationToken token, Stopwatch timer)
            {
                timer.Restart();
                //Thread.Sleep(dellay);
                await Task.Delay(dellay);
                Console.WriteLine($"\r\nElapsed {timer.Elapsed.Seconds} sec. Cooldown finish. Let's go back to work");
                Console.ResetColor();
                _StartTasksEvent?.Invoke(dellay, token);
            }
        }

        static void Main(string[] args) {

            Stopwatch _fullTimer = Stopwatch.StartNew();
            int _stopAllTimeout = 60;
            int _minDellay = 1;
            int _maxDellay = 5;
            //Let's configure
            ChainAsyncTASK scheduler = new ChainAsyncTASK(_stopAllTimeout, _minDellay, _maxDellay);
            Console.WriteLine($"All tasks will be closed after {_stopAllTimeout} seconds\r\n");

            //We ned to start first time manualy
            Task chainTasksAsync = Task.Run(async () => {
                var timer = Stopwatch.StartNew();
                int _sleep = scheduler.Random.Next(scheduler.MinSleepSeconds, scheduler.MaxSleepSeconds);
                Console.WriteLine($"\r\nFirst method start.Going to sleep {_sleep} sec");
                await Task.Delay(_sleep * 1000, scheduler.CancellationToken);
                Console.WriteLine($"\r\nFirst method end. Worked {timer.Elapsed.Seconds} s");
                await scheduler.SecondMethodAsync(scheduler.CancellationToken);

            }, scheduler.CancellationToken);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Starting main thread..");
            Console.ResetColor();
            Console.WriteLine($"{new string('_', 30)}\r\n");
            

            for (int i = 0; i < Int32.MaxValue; i++) {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(".");
                Console.ResetColor();
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }
    }
}
