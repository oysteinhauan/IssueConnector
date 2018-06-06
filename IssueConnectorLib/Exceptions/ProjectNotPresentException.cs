using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorWebService.Exceptions
{
    public class ProjectNotPresentException : Exception
    {
		public ProjectNotPresentException(string message, Exception e) : base(message, e)
		{

		}
    }
}
