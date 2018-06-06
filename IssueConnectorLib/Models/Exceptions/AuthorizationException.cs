using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Exceptions
{
	public class AuthorizationException : Exception
    {
		public AuthorizationException(string message, Exception e) : base(message, e)
		{

		}
    }
}
