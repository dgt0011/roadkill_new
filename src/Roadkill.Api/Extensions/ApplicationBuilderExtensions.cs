using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Roadkill.Api.Middleware;

namespace Roadkill.Api.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseJsonHealthChecks(this IApplicationBuilder app)
		{
			var options = new HealthCheckOptions
			{
				ResponseWriter = async (context, report) =>
				{
					var statusObject = new
					{
						Status = report.Status.ToString(),
						Errors = report.Entries.Select(error => new
						{
							HealthCheckName = error.Key,
							Description = error.Value.Description
						})
					};

					var json = JsonConvert.SerializeObject(statusObject);
					context.Response.ContentType = MediaTypeNames.Application.Json;
					await context.Response.WriteAsync(json);
				}
			};

			app.UseHealthChecks("/healthcheck", options);

			return app;
		}

		public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment environment)
		{
			app.UseExceptionHandler(new ExceptionHandlerOptions()
			{
				ExceptionHandler = new JsonExceptionMiddleware(environment).Invoke
			});

			return app;
		}

		public static IApplicationBuilder UseSwaggerWithReverseProxySupport(this IApplicationBuilder app)
		{
			//TODO: Fix
			//app.UseSwagger(config => config.PostProcess = (document, request) =>
			//{
			//	string pathBase = request.Headers["X-Forwarded-PathBase"].FirstOrDefault();
			//	document.BasePath = pathBase;
			//	document.Host = request.Headers["X-Forwarded-Host"].FirstOrDefault();
			//});

			app.UseSwagger();

			app.UseSwaggerUi(settings =>
			{
				settings.TransformToExternalPath = (route, request) =>
				{
					string pathBase = request.Headers["X-Forwarded-PathBase"].FirstOrDefault();

					if (!string.IsNullOrEmpty(pathBase))
						return $"{pathBase}{route}";

					return route;
				};
			});

			return app;
		}
	}
}
