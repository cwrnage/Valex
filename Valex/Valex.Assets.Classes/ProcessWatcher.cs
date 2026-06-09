using System;
using System.Diagnostics;
using System.Timers;

namespace Valex.Assets.Classes;

public class ProcessWatcher : IDisposable
{
	public delegate void OnProcessCreatedDelegate(object sender, Process process);

	private readonly Timer _timer;

	private readonly string _processname;

	private bool _disposed = false;

	private Process _process;

	public event OnProcessCreatedDelegate Created;

	public ProcessWatcher(string processName)
	{
		_processname = processName;
		_timer = new Timer();
		_timer.Elapsed += TimerOnElapsed;
		_timer.Start();
	}

	private void TimerOnElapsed(object sender, ElapsedEventArgs e)
	{
		Process[] processesByName = Process.GetProcessesByName(_processname);
		if (processesByName.Length == 1)
		{
			OnProcessCreated(processesByName[0]);
		}
	}

	protected virtual void OnProcessCreated(Process process)
	{
		_timer.Stop();
		_process = process;
		_process.EnableRaisingEvents = true;
		_process.Exited += delegate
		{
			_timer.Start();
		};
		this.Created?.Invoke(this, process);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_timer.Dispose();
			}
			_disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
