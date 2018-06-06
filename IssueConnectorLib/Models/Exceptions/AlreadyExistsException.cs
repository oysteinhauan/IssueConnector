using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Exceptions
{
    public class AlreadyExistsException : Exception
    {
		public AlreadyExistsException(string msg, Exception e) : base(msg, e) { }
    }
}
