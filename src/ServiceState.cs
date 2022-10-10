using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteService
{
	public enum ServiceState
	{
		Running,
		Stopped,
		Paused,
		StartPending,
		StopPending,
		PausePending,
		ContinuePending
	}
}
