using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace IssueConnectorLib.Models
{
    public interface IMessageFilter<T>
    {
		bool OfInterest(T context, string origin);
    }
}
