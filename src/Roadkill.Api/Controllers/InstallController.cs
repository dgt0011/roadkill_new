using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Core.Entities.Authorization;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[AllowAnonymous]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	public class InstallController : ControllerBase
	{
		private readonly UserManager<RoadkillIdentityUser> _userManager;

		public InstallController(UserManager<RoadkillIdentityUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpPost]
		[Route(nameof(CreateTestUser))]
		public async Task<ActionResult<string>> CreateTestUser()
		{
			var newUser = new RoadkillIdentityUser()
			{
				UserName = "admin@localhost",
				Email = "admin@localhost",
				EmailConfirmed = true
			};

			await _userManager.CreateAsync(newUser, "password");
			await _userManager.AddClaimAsync(newUser, RoadkillClaims.AdminClaim);

			return CreatedAtAction(nameof(CreateTestUser), newUser.Email);
		}
	}
}
