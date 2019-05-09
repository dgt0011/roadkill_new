﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Exceptions;
using Roadkill.Api.JWT;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Authorization;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize(Policy = PolicyNames.Admin)]
	public class UsersController : ControllerBase
	{
		public static readonly string EmailExistsError = "The email address already exists.";
		public static readonly string EmailDoesNotExistError = "The email address does not exist.";
		public static readonly string UserIsLockedOutError = "The user with the email address is already locked out.";
		private readonly UserManager<RoadkillUser> _userManager;
		private readonly IUserObjectsConverter _userObjectsConverter;

		public UsersController(
			UserManager<RoadkillUser> userManager,
				IUserObjectsConverter userObjectsConverter)
		{
			_userManager = userManager;
			_userObjectsConverter = userObjectsConverter;
		}

		[HttpGet]
		[Route(nameof(GetByEmail), Name = nameof(GetByEmail))]
		public async Task<ActionResult<UserResponse>> GetByEmail(string email)
		{
			RoadkillUser user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return NotFound(EmailDoesNotExistError);
			}

			UserResponse response = _userObjectsConverter.ConvertToUserResponse(user);

			return Ok(response);
		}

		[HttpGet]
		[Route(nameof(FindAll))]
		public ActionResult<IEnumerable<UserResponse>> FindAll()
		{
			IEnumerable<RoadkillUser> roadkillUsers = _userManager.Users.ToList();
			IEnumerable<UserResponse> responses =
				roadkillUsers.Select(u => _userObjectsConverter.ConvertToUserResponse(u));

			return Ok(responses);
		}

		[HttpGet]
		[Route(nameof(FindUsersWithClaim))]
		public async Task<ActionResult<IEnumerable<UserResponse>>> FindUsersWithClaim(string claimType, string claimValue)
		{
			var claim = new Claim(claimType, claimValue);
			IList<RoadkillUser> usersForClaim = await _userManager.GetUsersForClaimAsync(claim);

			IEnumerable<UserResponse> responses =
				usersForClaim.Select(u => _userObjectsConverter.ConvertToUserResponse(u));

			return Ok(responses);
		}

		[HttpPost]
		[Route(nameof(CreateAdmin))]
		public async Task<ActionResult<string>> CreateAdmin(UserRequest userRequest)
		{
			var user = await _userManager.FindByEmailAsync(userRequest.Email);
			if (user != null)
			{
				return BadRequest(EmailExistsError);
			}

			var newUser = new RoadkillUser()
			{
				UserName = userRequest.Email,
				Email = userRequest.Email,
				EmailConfirmed = true
			};

			IdentityResult result = await _userManager.CreateAsync(newUser, userRequest.Password);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to create admin user {userRequest.Email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Admin));

			return CreatedAtAction(nameof(CreateEditor), userRequest.Email);
		}

		[HttpPost]
		[Route(nameof(CreateEditor))]
		public async Task<ActionResult<string>> CreateEditor(UserRequest userRequest)
		{
			var user = await _userManager.FindByEmailAsync(userRequest.Email);
			if (user != null)
			{
				return BadRequest(EmailExistsError);
			}

			var newUser = new RoadkillUser()
			{
				UserName = userRequest.Email,
				Email = userRequest.Email,
				EmailConfirmed = true
			};

			IdentityResult result = await _userManager.CreateAsync(newUser, userRequest.Password);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to create editor user {userRequest.Email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, RoleNames.Editor));

			return CreatedAtAction(nameof(CreateEditor), userRequest.Email);
		}

		[HttpPost]
		[Route(nameof(DeleteUser))]
		public async Task<ActionResult<string>> DeleteUser([FromBody]string email)
		{
			RoadkillUser user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return NotFound(EmailDoesNotExistError);
			}

			if (user.LockoutEnabled)
			{
				return BadRequest(UserIsLockedOutError);
			}

			user.LockoutEnd = DateTime.MaxValue;
			user.LockoutEnabled = true;

			IdentityResult result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				throw new ApiException($"Unable to delete user {email} - UserManager call failed." + string.Join("\n", result.Errors));
			}

			return NoContent();
		}
	}
}
