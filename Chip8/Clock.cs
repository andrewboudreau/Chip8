using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Chip8
{
    public class Clock : IDisposable
    {
        const int ticksPer500Hz = 20_000;
        const int ticksPer60Hz = 166_666;

        private readonly Task thread;
        private readonly Action tick60Hz;
        private readonly Action tick500Hz;
        private readonly Stopwatch stopwatch;

        private long last60Hz = 0;
        private long last500Hz = 0;

        private CancellationTokenSource cancellationTokenSource;

        public Clock(Action tick60Hz, Action tick500Hz, CancellationToken cancellationToken)
        {
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            stopwatch = Stopwatch.StartNew();

            this.tick60Hz = tick60Hz;
            this.tick500Hz = tick500Hz;

            thread = Task.Run(Loop, cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            thread.Wait(TimeSpan.FromMilliseconds(100));
        }

        private void Loop()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (stopwatch.ElapsedTicks - last500Hz > ticksPer500Hz)
                {
                    last500Hz = stopwatch.ElapsedTicks;
                    tick500Hz();
                }

                if (stopwatch.ElapsedTicks - last60Hz > ticksPer60Hz)
                {
                    last60Hz = stopwatch.ElapsedTicks;
                    tick60Hz();
                }

                Thread.Sleep(TimeSpan.FromTicks(stopwatch.ElapsedTicks - last500Hz));
            }
        }
    }
}