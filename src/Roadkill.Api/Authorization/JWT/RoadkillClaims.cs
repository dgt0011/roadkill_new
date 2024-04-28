using System.Security.Claims;
using Roadkill.Api.Authorization.Roles;

namespace Roadkill.Api.Authorization.JWT
{
	public static class RoadkillClaims
	{
		public static Claim AdminClaim => new(ClaimTypes.Role, AdminRoleDefinition.Name);
		public static Claim EditorClaim => new(ClaimTypes.Role, EditorRoleDefinition.Name);
	}
}
