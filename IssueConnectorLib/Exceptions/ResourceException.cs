using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Exceptions
{
    public class ResourceException : Exception
    {
		public ResourceException(string message) : base(message)
		{

		}
	}
}
