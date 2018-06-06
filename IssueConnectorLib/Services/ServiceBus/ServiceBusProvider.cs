using System;
using System.Diagnostics;
using GreenPipes;
using IssueConnectorLib.Models;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;

namespace IssueConnectorLib.Services.ServiceBus
{
    public class ServiceBusProvider
    {
        public static IBusControl GetRabbitMqPublisherBus(string uri)
        {
			Debug.WriteLine("Creating publisher with uri " + uri);
            return Bus.Factory.CreateUsingRabbitMq(x => {
                x.Host(new Uri(uri), host => { });
            });
        }

		public static IBusControl GetRabbitMqConsumerBus(IResourceConsumer consumer, string uri, string systemName, string systemId)
		{
			Debug.WriteLine("Creating consumer bus for system " + systemName + " with id " + systemId);
			return Bus.Factory.CreateUsingRabbitMq(x =>
			{
				var host = x.Host(new Uri(uri), h => { });

				x.ReceiveEndpoint(host, systemName + "Consumer_" + systemId, e =>
					e.Instance(consumer));
			});
		}

		//public static IBusControl GetAzureBus(string hostUri, string accessKeyname, string accessKey)
		//{
		//	return Bus.Factory.(sbc =>
		//	{
		//		var host = sbc.Host(new Uri(hostUri), h =>
		//		{
		//			h.OperationTimeout = TimeSpan.FromSeconds(5);
		//			h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(accessKeyname, accessKey);
		//		});

		//		sbc.ReceiveEndpoint(host, "command_queue", ep =>
		//		{
		//			ep.SubscribeMessageTopics = true;
		//			ep.UseRetry(Retry.Incremental(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
		//		});
		//	});
	//}
	}
}