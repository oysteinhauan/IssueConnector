using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.EventListeners
{
    public interface IEventListener
    {
		Task ConnectAsync();
		void Stop();

	}
}
