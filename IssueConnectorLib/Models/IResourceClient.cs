using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models
{
	public interface IResourceClient<T>
	{
		T RestClient { get; set; }
		Task Connect();
		Task Reconnect();
    }
}
