using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraService.Models
{
    public class ConfigSettings
    {
		public string Username { get; set; }
		public string Password { get; set; }
		public string ServiceUri { get; set; }
		public string HomeUri { get; set; }
		public string UserColumnName { get; set; }
	    public string RabbitMqUri { get; set; }
	    public string SystemName { get; set; }
	}
}
