using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace StandardErrorMessages
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc(options =>
				{
					options.Filters.Add<ValidateModelAttribute>();
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.Configure<ApiBehaviorOptions>(o =>
			{
				o.InvalidModelStateResponseFactory = ctx =>
					new JsonResult(ErrorResponse.InvalidModel(ctx.ModelState));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app
				.UseStatusCodePages(async ctx => { await HandleError(ctx.HttpContext); })
				.UseExceptionHandler(new ExceptionHandlerOptions() { ExceptionHandler = HandleError })
				.UseMvc();
		}

		private async Task HandleError(HttpContext ctx)
		{
			ErrorResponse err = null;
			var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;

			if (ctx.Response.StatusCode == (int)HttpStatusCode.NotFound)
			{
				err = ErrorResponse.InvalidPath(ctx.Request.Path);
			}
			else if (ex != null)
			{
				ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				err = ErrorResponse.Exception(ex);
			}

			if (err == null) return;

			ctx.Response.ContentType = ErrorResponse.ContentType;

			using (var writer = new StreamWriter(ctx.Response.Body))
			{
				new JsonSerializer().Serialize(writer, err);
				await writer.FlushAsync().ConfigureAwait(false);
			}
		}
	}
}
