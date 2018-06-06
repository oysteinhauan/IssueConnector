using EasyAccessService.Models;
using IssueConnectorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Mapper
{
    public interface IEasyAccessStandardInstanceMapper
    {
		Task<IStandardInstance> ToStandardObject(Topic topic, List<Comment> comments);

		Task<Topic> ToNewSpecialObject(IStandardInstance instance);

		Task<Topic> ToUpdatedSpecialObject(IStandardInstance instance, Topic oldTopic);
	}
}
