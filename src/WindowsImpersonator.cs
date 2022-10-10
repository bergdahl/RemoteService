using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace RemoteService
{

	public class WindowsImpersonator
	{
		public const int LOGON32_LOGON_INTERACTIVE = 2;
		public const int LOGON32_LOGON_NETWORK = 3;
		public const int LOGON32_PROVIDER_DEFAULT = 0;

		WindowsImpersonationContext impersonationContext;

		[DllImport("advapi32.dll")]
		public static extern bool LogonUser(String lpszUserName,
			String lpszDomain,
			String lpszPassword,
			int dwLogonType,
			int dwLogonProvider,
			ref IntPtr phToken);
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int DuplicateToken(IntPtr hToken,
			int impersonationLevel,
			ref IntPtr hNewToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool RevertToSelf();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool CloseHandle(IntPtr handle);

		public bool ImpersonateValidUser(String userName, String domain, String password)
		{
			WindowsIdentity tempWindowsIdentity;
			IntPtr token = IntPtr.Zero;
			IntPtr tokenDuplicate = IntPtr.Zero;

			if (RevertToSelf())
			{
				bool result = LogonUser(userName, domain, password, LOGON32_LOGON_NETWORK,
					LOGON32_PROVIDER_DEFAULT, ref token);
				int error = Marshal.GetLastWin32Error();
				if (result)
				{
					if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
					{
						tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
						impersonationContext = tempWindowsIdentity.Impersonate();
						if (impersonationContext != null)
						{
							CloseHandle(token);
							CloseHandle(tokenDuplicate);
							return true;
						}
					}
				}
			}
			if (token != IntPtr.Zero)
				CloseHandle(token);
			if (tokenDuplicate != IntPtr.Zero)
				CloseHandle(tokenDuplicate);
			return false;
		}

		public void UndoImpersonation()
		{
			if (impersonationContext != null)
				impersonationContext.Undo();
		}
	}
}
