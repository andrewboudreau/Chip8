using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Chip8
{
	public class Clock
	{
		// = (TimeSpan.TicksPerSecond / 500)
		const int ticksPer500Hz = 20_000;

		// = (TimeSpan.TicksPerSecond / 60)
		const int ticksPer60Hz = 166_666;

		private readonly Task thread;
		private readonly Action tick60Hz;
		private readonly Action tick500Hz;
		private readonly Stopwatch stopwatch;

		private long last60Hz = 0;
		private long last500Hz = 0;
		public Clock(Action tick60Hz, Action tick500Hz)
		{
			stopwatch = Stopwatch.StartNew();
			this.tick60Hz = tick60Hz;
			this.tick500Hz = tick500Hz;

			Running = true;
			thread = Task.Run(Loop);
		}

		public bool Running { get; set; }

		private void Loop()
		{
			while (Running)
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

				var sleepFor = TimeSpan.FromTicks(stopwatch.ElapsedTicks - last500Hz);
				Thread.Sleep(sleepFor);
			}
		}
	}
}