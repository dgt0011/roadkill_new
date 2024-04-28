using System;
using Marten.Schema;

namespace Roadkill.Core.Entities.Authorization
{
	public class UserRefreshToken
	{
		[Identity]
		public string RefreshToken { get; set; }

		public string JwtToken { get; set; }
		public DateTime CreationDate { get; set; }
		public string IpAddress { get; set; }
		public string Email { get; set; }
	}
}
