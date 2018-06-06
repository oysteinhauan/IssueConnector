using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace IssueConnectorLib.Models.SystemInfo
{
    public class MachineInfo
    {
		public static string GetMacAddress()
		{
			return NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
		}
    }
}
