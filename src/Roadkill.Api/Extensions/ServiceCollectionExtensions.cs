using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Asp.Versioning;
using MailKit;
using MailKit.Net.Smtp;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Roadkill.Api.Authorization.JWT;
using Roadkill.Api.Authorization.Policies;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Api.Settings;
using Roadkill.Api.Swagger;
using Roadkill.Core.Entities.Authorization;
using Roadkill.Core.Extensions;
using Roadkill.Core.Repositories;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Roadkill.Text.TextMiddleware;
using Scrutor;
using Swashbuckle.AspNetCore.SwaggerGen;
using AdminRoleDefinition = Roadkill.Api.Authorization.Roles.AdminRoleDefinition;
using EditorRoleDefinition = Roadkill.Api.Authorization.Roles.EditorRoleDefinition;

namespace Roadkill.Api.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ScanAndRegisterApi(this IServiceCollection services)
		{
			services.Scan(scan => scan
				.FromAssemblyOf<JwtSettings>()
				.AddClasses()
				.UsingRegistrationStrategy(RegistrationStrategy.Skip)
				.AsMatchingInterface()
				.WithTransientLifetime());

			return services;
		}

		public static IServiceCollection AddMailkit(this IServiceCollection services)
		{
			services.AddScoped<IMailTransport, SmtpClient>();
			return services;
		}

		public static IServiceCollection AddMarkdown(this IServiceCollection services)
		{
			services.AddScoped<TextSettings>();
			services.AddScoped<IHtmlWhiteListProvider, HtmlWhiteListProvider>();
			services.AddScoped<ITextMiddlewareBuilder>(TextMiddlewareBuilder.Default);
			return services;
		}

		public static IServiceCollection AddJwtDefaults(this IServiceCollection services, IConfiguration configuration, ILogger logger)
		{
			var jwtSettings = services.AddConfigurationOf<JwtSettings>(configuration);
			if (jwtSettings == null)
			{
				logger.LogError($"No JWT settings were found. Do you have a JWT section in your configuration file?");
				throw new InvalidOperationException("No JWT settings were found. Do you have a JWT section in your configuration file?");
			}

			if (jwtSettings.Password.Length < 20)
			{
				logger.LogError($"The JWT.Password is under 20 characters in length.");
				throw new InvalidOperationException("The JWT.Password setting is under 20 characters in length.");
			}

			if (jwtSettings.ExpiresMinutes < 1)
			{
				logger.LogError($"The JWT.JwtExpiresMinutes is {jwtSettings.ExpiresMinutes}.");
				throw new InvalidOperationException("The JWT.JwtExpiresMinutes is setting should be 1 or greater.");
			}

			if (jwtSettings.RefreshTokenExpiresDays < 1)
			{
				logger.LogError($"The JWT.RefreshTokenExpiresDays is {jwtSettings.RefreshTokenExpiresDays}.");
				throw new InvalidOperationException("The JWT.RefreshTokenExpiresDays is setting should be 1 or greater.");
			}

			services.AddSingleton(jwtSettings);
			services.AddScoped<IJwtTokenService>(provider =>
			{
				var settings = provider.GetRequiredService<JwtSettings>();
				var tokenHandler = new JwtSecurityTokenHandler();
				var repository = provider.GetRequiredService<IUserRefreshTokenRepository>();
				var tokenValidationParams = provider.GetRequiredService<TokenValidationParameters>();

				return new JwtTokenService(settings, tokenHandler, repository, tokenValidationParams);
			});

			byte[] password = Encoding.ASCII.GetBytes(jwtSettings.Password);
			var jwtTokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(password),
				ValidateIssuer = false,
				ValidateAudience = false,
				RequireExpirationTime = false,
				ValidateLifetime = true
			};

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = false;
				options.SaveToken = true;
				options.TokenValidationParameters = jwtTokenValidationParameters;
			});

			services.AddSingleton<TokenValidationParameters>(jwtTokenValidationParameters);
			services.AddSingleton<IUserRoleDefinition, AdminRoleDefinition>();
			services.AddSingleton<IUserRoleDefinition, EditorRoleDefinition>();
			services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();

			void ConfigureJwtClaimsPolicies(AuthorizationOptions options)
			{
				foreach (string policyName in PolicyNames.AllPolicies)
				{
					options.AddPolicy(policyName, policy =>
					{
						policy.Requirements.Add(new RoadkillPolicyRequirement(policyName));
					});
				}
			}
			services.AddAuthorization(ConfigureJwtClaimsPolicies);

			return services;
		}

		public static IServiceCollection AddIdentityDefaults(this IServiceCollection services)
		{
			services.AddIdentity<RoadkillIdentityUser, IdentityRole>(
					options =>
					{
						options.Password.RequireDigit = false;
						options.Password.RequiredUniqueChars = 0;
						options.Password.RequireLowercase = false;
						options.Password.RequiredLength = 0;
						options.Password.RequireNonAlphanumeric = false;
						options.Password.RequireUppercase = false;
					})
				.AddMartenStores<RoadkillIdentityUser, IdentityRole>()
				.AddDefaultTokenProviders();

			return services;
		}

		public static IServiceCollection AddMvcAndVersionedSwagger(this IServiceCollection services)
		{
			services
				.AddMvcCore()
				.AddDataAnnotations();

			services.AddControllers();

			services.AddApiVersioning(options =>
				{
					options.ReportApiVersions = true;
					options.ApiVersionReader = new UrlSegmentApiVersionReader();
					options.AssumeDefaultVersionWhenUnspecified = true;
				})
				.AddMvc()
				.AddApiExplorer(options =>
				{
					options.GroupNameFormat = "'v'VVV";
					options.SubstituteApiVersionInUrl = true;
				});

			services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
			services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());

			services.AddOpenApiDocument(document =>
			{
				document.DocumentName = "v3";
			});

			return services;
		}

		public static IServiceCollection AddAutoMapperForApi(this IServiceCollection services)
		{
			services.AddAutoMapper(typeof(JwtSettings));
			return services;
		}
	}
}
