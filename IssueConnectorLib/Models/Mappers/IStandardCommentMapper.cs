using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Mappers
{
    public interface IStandardCommentMapper<STANDARD, SPECIAL>
    {
		Task<STANDARD> ToStandardComment(SPECIAL instance, string instanceId = null);

		Task<SPECIAL> ToNewSpecialComment(STANDARD instance);

		Task<SPECIAL> ToUpdatedSpecialComment(STANDARD instance, SPECIAL oldSpecial);
	}
}
