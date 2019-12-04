using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.Common;
using Roadkill.Api;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Core.Entities;
using Roadkill.Core.Entities.Authorization;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Api.Controllers
{
	public class IntegrationTestsWebFactory : WebApplicationFactory<Startup>
	{
		private static Dictionary<string, string> _testConfigValues = new Dictionary<string, string>
		{
			{ "Postgres:ConnectionString", "host=localhost;port=5432;database=roadkilltests;username=roadkill;password=roadkill;" },
			{ "Smtp:Host", "smtp.gmail.com" },
			{ "Smtp:Port", "587" },
			{ "Smtp:UseSsl", "true" },
			{ "Smtp:Username", "bob" },
			{ "Smtp:Password", "mypassword" },
			{ "Jwt:Password", "12345678901234567890" },
			{ "Jwt:ExpiresDays", "7" }
		};

		public ILogger<IDocumentStore> Logger { get; set; }

		/// <summary>
		/// Gets or sets the ITestOutputHelper which enables logging to the XUnit console
		/// via an MartinCostello.Logging.XUnit and an ILogger.
		/// </summary>
		public ITestOutputHelper TestOutputHelper { get; set; }

		public RoadkillIdentityUser AdminUser { get; set; }

		public string AdminUserPassword { get; set; }

		public RoadkillIdentityUser EditorUser { get; set; }

		public string EditorUserPassword { get; set; }

		protected override TestServer CreateServer(IWebHostBuilder builder)
		{
			// Google Cloud Docker usage
			TestOutputHelper.WriteLine("Directory: " + Directory.GetCurrentDirectory());
			if (Directory.GetCurrentDirectory().Contains("workspace/") )
			{
				_testConfigValues["Postgres:ConnectionString"] = "host=roadkill-postgres;port=5432;database=roadkilltests;username=roadkill;password=roadkill;";
			}

			var server = base.CreateServer(builder);
			var provider = server.Host.Services;

			Logger = provider.GetService<ILogger<IDocumentStore>>();

			// Clean the postgres database
			var documentStore = provider.GetService<IDocumentStore>();

			using (var session = documentStore.LightweightSession())
			{
				int count = session.Query<RoadkillIdentityUser>().Count();
				Logger.LogInformation($"Found {count} RoadkillUsers");

				count = session.Query<Page>().Count();
				Logger.LogInformation($"Found {count} Pages");
			}

			documentStore.Advanced.Clean.DeleteAllDocuments();

			// Create two users
			var manager = provider.GetService<UserManager<RoadkillIdentityUser>>();
			CreateAdminUser(manager);
			CreateEditorUser(manager);

			return server;
		}

		protected override IWebHostBuilder CreateWebHostBuilder()
		{
			var configBuilder = new ConfigurationBuilder();
			configBuilder.AddInMemoryCollection(_testConfigValues);
			var config = configBuilder.Build();

			var builder = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.ConfigureLogging(loggingBuilder => loggingBuilder.AddXUnit(TestOutputHelper))
				.UseConfiguration(config)
				.UseStartup<Startup>();

			return builder;
		}

		private void CreateAdminUser(UserManager<RoadkillIdentityUser> manager)
		{
			AdminUser = new RoadkillIdentityUser()
			{
				UserName = "admin@example.org",
				Email = "admin@example.org",
				EmailConfirmed = true
			};
			AdminUserPassword = "Password1234567890";

			var result = manager.CreateAsync(AdminUser, AdminUserPassword).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create admin user - " + errors);
			}

			manager.AddClaimAsync(AdminUser, RoadkillClaims.AdminClaim).GetAwaiter()
				.GetResult();
		}

		private void CreateEditorUser(UserManager<RoadkillIdentityUser> manager)
		{
			EditorUser = new RoadkillIdentityUser()
			{
				UserName = "editor@example.org",
				Email = "editor@example.org",
				EmailConfirmed = true
			};
			EditorUserPassword = "Password1234567890";

			var result = manager.CreateAsync(EditorUser, EditorUserPassword).GetAwaiter().GetResult();
			if (!result.Succeeded)
			{
				string errors = string.Join("\n", result.Errors.ToList().Select(x => x.Description));
				throw new Exception("Failed to create editor user - " + errors);
			}

			manager.AddClaimAsync(EditorUser, RoadkillClaims.EditorClaim).GetAwaiter()
				.GetResult();
		}
	}
}
