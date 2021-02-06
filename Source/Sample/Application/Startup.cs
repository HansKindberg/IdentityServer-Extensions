using System;
using Application.Models.Views.Shared;
using HansKindberg.IdentityServer.Builder;
using HansKindberg.IdentityServer.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application
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
			applicationBuilder.UseDefault();
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			services.Configure(this.Configuration, this.HostEnvironment, () =>
			{
				services.AddScoped<LayoutViewModel>();
			});
		}

		#endregion
	}
}