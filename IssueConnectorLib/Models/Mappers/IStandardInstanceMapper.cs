using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IssueConnectorLib.Models.Mappers
{
	public interface IStandardInstanceMapper<STANDARD, SPECIAL>
	{
		Task<STANDARD> ToStandardObject(SPECIAL instance);

		Task<SPECIAL> ToNewSpecialObject(STANDARD instance);

		Task<SPECIAL> ToUpdatedSpecialObject(STANDARD instance, SPECIAL oldSpecial);


	}
}
