using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteService
{
	public class AdminShareHelper
	{
		private enum ResourceScope
		{
			RESOURCE_CONNECTED = 1,
			RESOURCE_GLOBALNET,
			RESOURCE_REMEMBERED,
			RESOURCE_RECENT,
			RESOURCE_CONTEXT
		}
		private enum ResourceType
		{
			RESOURCETYPE_ANY,
			RESOURCETYPE_DISK,
			RESOURCETYPE_PRINT,
			RESOURCETYPE_RESERVED
		}
		private enum ResourceUsage
		{
			RESOURCEUSAGE_CONNECTABLE = 0x00000001,
			RESOURCEUSAGE_CONTAINER = 0x00000002,
			RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
			RESOURCEUSAGE_SIBLING = 0x00000008,
			RESOURCEUSAGE_ATTACHED = 0x00000010
		}
		private enum ResourceDisplayType
		{
			RESOURCEDISPLAYTYPE_GENERIC,
			RESOURCEDISPLAYTYPE_DOMAIN,
			RESOURCEDISPLAYTYPE_SERVER,
			RESOURCEDISPLAYTYPE_SHARE,
			RESOURCEDISPLAYTYPE_FILE,
			RESOURCEDISPLAYTYPE_GROUP,
			RESOURCEDISPLAYTYPE_NETWORK,
			RESOURCEDISPLAYTYPE_ROOT,
			RESOURCEDISPLAYTYPE_SHAREADMIN,
			RESOURCEDISPLAYTYPE_DIRECTORY,
			RESOURCEDISPLAYTYPE_TREE,
			RESOURCEDISPLAYTYPE_NDSCONTAINER
		}
		[StructLayout(LayoutKind.Sequential)]
		private struct NETRESOURCE
		{
			public ResourceScope oResourceScope;
			public ResourceType oResourceType;
			public ResourceDisplayType oDisplayType;
			public ResourceUsage oResourceUsage;
			public string sLocalName;
			public string sRemoteName;
			public string sComments;
			public string sProvider;
		}

		[DllImport("mpr.dll")]
		private static extern int WNetAddConnection2
			(ref NETRESOURCE oNetworkResource, string sPassword,
			string sUserName, int iFlags);

		[DllImport("mpr.dll")]
		private static extern int WNetCancelConnection2
			(string sLocalName, uint iFlags, int iForce);

		public static int Connect(string host, string userName, string password)
		{
			string networkPath = String.Format("\\\\{0}\\Admin$", host);
			NETRESOURCE networkResource = new NETRESOURCE();
			networkResource.oResourceType = ResourceType.RESOURCETYPE_ANY;
			networkResource.sRemoteName = networkPath;
			return WNetAddConnection2(ref networkResource, password, userName, 0);
		}

		public static int Disconnect(string host)
		{
			string networkPath = String.Format("\\\\{0}\\Admin$", host);
			return WNetCancelConnection2(networkPath, 0, 0);
		}
	}
}
