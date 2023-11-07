using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteService
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("RemoteService version 1.2");
			Console.WriteLine("Copyright 2013-2023, Bergdahl IT AB");

			if (args.Length < 3)
			{
				DisplayHelp();
				return;
			}

			string host = args[0];
			string action = args[1];
			string serviceName = args[2];
			string userName = args.Length > 3 ? args[3] : "";
			string password = args.Length > 4 ? args[4] : "";

			Console.WriteLine("Using arguments {0} {1} {2} {3} {4}",
								host,
								action,
								serviceName,
								userName,
								password);
			ServiceHelper serviceHelper = new ServiceHelper(host, serviceName, userName, password);
			ReturnValue result = serviceHelper.ControlService(action);
			Console.WriteLine("Exiting with code {0} ({1})", result, (int)result);
			Environment.ExitCode = (int)result;
		}

		private static void DisplayHelp()
		{
			Console.WriteLine("RemoteService.exe <HOST> <ACTION> <SERVICENAME> [USERNAME] [PASSWORD]");
			Console.WriteLine("Possible Actions: STOP, START");
			Console.WriteLine("Username and password are optional values");
		}
	}
}
