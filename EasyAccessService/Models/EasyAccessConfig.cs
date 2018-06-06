using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Models
{
    public class EasyAccessConfig
    {
		public string Username { get; set; }
		public string Password { get; set; }
		public string ServiceUri { get; set; }
		public string HomeUri { get; set; }
		public string UserColumnName { get; set; }
		public string SignalServiceUri { get; set; }
		public string EasyAccessTopic { get; set; }
	    public string SystemName { get; set; }
	    public string RabbitMqUri { get; set; }
	}
}
