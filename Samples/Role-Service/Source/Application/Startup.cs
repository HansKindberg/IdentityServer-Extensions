using System;
using HansKindberg.RoleService.Models.Configuration;
using HansKindberg.Web.Authorization.Builder.Extentsions;
using HansKindberg.Web.Authorization.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService
{
	public class Startup
	{
		#region Constructors

		public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.HostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
		}

		#endregion

		#region Properties

		protected internal virtual IConfiguration Configuration { get; }
		protected internal virtual IHostEnvironment HostEnvironment { get; }

		#endregion

		#region Methods

		public virtual void Configure(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			var exceptionHandling = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<ExceptionHandlingOptions>>().Value;

			if(exceptionHandling.DeveloperExceptionPage)
				applicationBuilder.UseDeveloperExceptionPage();
			else
				applicationBuilder.UseExceptionHandler(exceptionHandling.Path);

			applicationBuilder.UseHttpsRedirection();
			applicationBuilder.UseRouting();
			applicationBuilder.UseAuthentication();
			applicationBuilder.UseExtendedAuthorization();
			applicationBuilder.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			if(services == null)
				throw new ArgumentNullException(nameof(services));

			var jwtBearerConfigurationSection = this.Configuration.GetSection(ConfigurationKeys.JwtBearerPath);

			services.Configure<ExceptionHandlingOptions>(this.Configuration.GetSection(ConfigurationKeys.ExceptionHandlingPath));
			services.Configure<JsonOptions>(this.Configuration.GetSection(ConfigurationKeys.JsonOptionsPath));
			services.Configure<JwtBearerOptions>(jwtBearerConfigurationSection);

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options => jwtBearerConfigurationSection.Bind(options));

			services.AddExtendedAuthorization(this.Configuration);
			services.Configure<AuthorizationOptions>(options =>
			{
				options.AddPolicy("scope", policy =>
				{
					policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
						.RequireAuthenticatedUser()
						.RequireClaim("scope", "role-service");
				});
			});

			services.AddControllers();
		}

		#endregion
	}
}