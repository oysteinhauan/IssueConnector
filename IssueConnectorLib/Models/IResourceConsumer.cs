using IssueConnectorLib.Models.Contracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models
{
    public interface IResourceConsumer : IConsumer<IStandardInstance>, IConsumer<IStandardViewpoint>, IConsumer<IStandardComment>, IConsumer<IStandardAttachment>, IConsumer<IStandardIdentifierMap>
	{
		Task StartConsumerAsync();
		Task<IStandardInstance> HandleCreation(IStandardInstance instance);
		Task<IStandardInstance> HandleUpdate(IStandardInstance instance);
		Task HandleDeletion(IStandardInstance instance);
		Task StopConsumerAsync();
    }
}
