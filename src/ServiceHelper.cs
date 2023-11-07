using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace RemoteService
{
	public class ServiceHelper
	{
		protected string host;
		protected string serviceName;
		protected string userName;
		protected string password;

		public ServiceHelper(string host, string serviceName, string userName, string password)
		{
			this.host = host;
			this.serviceName = serviceName;
			this.userName = userName;
			this.password = password;
		}

		public ReturnValue ControlService(string action)
		{
			ReturnValue result = ReturnValue.UnknownFailure;
			try
			{
				if (this.userName != "")
				{
					// Authenticate to the Admin$ share, this resolves any auth problems.
					// We ignore the result as auth errors will be catched below anyway
					int returnValue = AdminShareHelper.Connect(this.host, this.userName, this.password);
					Console.WriteLine("Admin$ logon result: {0:x} ({0})", returnValue);
				}
				switch (action.ToUpper())
				{
					case "START":
						result = StartService();
						break;
					case "STOP":
						result = StopService();
						break;
					default:
						throw new ArgumentException("Action " + action + " is not supported");
				}
			}
			catch (Exception ex)
			{
				while (ex != null)
				{
					Console.WriteLine(ex.ToString());
					result = ReturnValue.UnknownFailure;
					ex = ex.InnerException;
				}
			}
			finally
			{
				AdminShareHelper.Disconnect(this.host);
			}
			return result;
		}

		public ReturnValue StartService()
		{
			ServiceController controller = new ServiceController(this.serviceName, this.host);
			ReturnValue result = ReturnValue.UnknownFailure;
			try
			{
				Console.WriteLine("Initial service state is {0}", controller.Status);
				switch (controller.Status)
				{
					case ServiceControllerStatus.Running:
						return ReturnValue.Success;
					default:
						DateTime timeout = DateTime.Now + TimeSpan.FromMinutes(5);
						Console.WriteLine("Starting service {0}", this.serviceName);
						try
						{
							controller.Start();
						}
						catch (InvalidOperationException) {}
						WaitForStatus(controller, ServiceControllerStatus.Running, timeout);
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			Console.WriteLine("Final service state is {0}", controller.Status);
			result = controller.Status == ServiceControllerStatus.Running ? ReturnValue.Success : ReturnValue.ServiceRequestTimeout;
			return result;
		}

		public ReturnValue StopService()
		{
			ServiceController controller = new ServiceController(this.serviceName, this.host);
            int processId = GetProcessIdForServiceName();

            ReturnValue result = ReturnValue.UnknownFailure;
			try
			{
				Console.WriteLine("Initial service state is {0}", controller.Status);
				switch (controller.Status)
				{
					case ServiceControllerStatus.Stopped:
						return ReturnValue.Success;
					default:
						DateTime timeout = DateTime.Now + TimeSpan.FromMinutes(5);
						Console.WriteLine("Stopping service {0}", this.serviceName);
						try
						{
							controller.Stop();
						}
						catch (InvalidOperationException ex)
						{
							Console.WriteLine(ex.Message);
						}
						WaitForStatus(controller, ServiceControllerStatus.Stopped, timeout);
                        WaitForProcessExit(processId, DateTime.Now + TimeSpan.FromMinutes(1));                        
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.GetType().Name);
				Console.WriteLine(ex.Message);
			}
			Console.WriteLine("Final service state is {0}", controller.Status);
			result = controller.Status == ServiceControllerStatus.Stopped ? ReturnValue.Success : ReturnValue.ServiceRequestTimeout;
			return result;
		}

		private ServiceControllerStatus WaitForStatus(ServiceController controller, ServiceControllerStatus status, DateTime timeout)
		{
			Console.WriteLine("Waiting for state {0}", status);
			ServiceControllerStatus result = controller.Status;	
			while (controller.Status != status)
			{
				if (DateTime.Now > timeout)
				{
					throw new System.ServiceProcess.TimeoutException(String.Format("Timout waiting for service, state is {0}", controller.Status));
				}
				Thread.Sleep(5000);
				controller.Refresh();
				result = controller.Status;
				Console.WriteLine("Current state is {0}", result);
			}
			return result;
		}

        private void WaitForProcessExit(int processId, DateTime timeout)
        {
            bool running = true;
            if (processId == 0)
            {
                Console.WriteLine("Process id is 0, exiting");
            }
            Console.WriteLine("Waiting for process exit for {0}", processId);
            while (running)
            {
                if (DateTime.Now > timeout)
                {
                    throw new System.ServiceProcess.TimeoutException(String.Format("Timout waiting for process exit"));
                }

                try
                {
	                var process = Process.GetProcessById(processId, this.host);
	                Thread.Sleep(500);
                }
                catch
                {
	                // Process is not available
	                running = false;
                }
            }
        }

        private int GetProcessIdForServiceName()
        {
            int result = 0;

            try
            {
                ConnectionOptions connectOptions = null;
                string connectString = String.Format(@"\\{0}\root\cimv2", this.host);
                if (!String.IsNullOrEmpty(userName))
                {
                    connectOptions = new ConnectionOptions();
                    connectOptions.Username = this.userName;
                    connectOptions.Password = this.password;
                    connectOptions.Impersonation = ImpersonationLevel.Impersonate;
                    connectOptions.Authentication = AuthenticationLevel.Default;
                    connectOptions.Timeout = new TimeSpan(0, 0, 30);
                    connectOptions.EnablePrivileges = true;
                }
                ManagementScope scope = new ManagementScope(connectString, connectOptions);
                ObjectQuery query = new ObjectQuery(String.Format("SELECT * FROM Win32_Service WHERE Name='{0}'", this.serviceName));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    result = Convert.ToInt32(obj["ProcessId"]);
                }
            }
            catch
            {
                // Ignore
            }
            return result;
        }

        public static string[] SplitArguments(string commandLine)
		{
			var parmChars = commandLine.ToCharArray();
			var inSingleQuote = false;
			var inDoubleQuote = false;
			for (var index = 0; index < parmChars.Length; index++)
			{
				if (parmChars[index] == '"' && !inSingleQuote)
				{
					inDoubleQuote = !inDoubleQuote;
					parmChars[index] = '\n';
				}
				if (parmChars[index] == '\'' && !inDoubleQuote)
				{
					inSingleQuote = !inSingleQuote;
					parmChars[index] = '\n';
				}
				if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
					parmChars[index] = '\n';
			}
			return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
