using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Models
{
	public class EasyAccessToken
	{

		public EasyAccessToken() { }

		public string Access_Token {get; set; }

		public string Token_Type { get; set; }

		public string Expires_In { get; set; }

    }
}
