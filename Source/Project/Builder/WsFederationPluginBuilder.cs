using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsk.WsFederation.Configuration;
using Rsk.WsFederation.EntityFramework.DbContexts;

namespace HansKindberg.IdentityServer.Builder
{
	public class WsFederationPluginBuilder : IWsFederationPluginBuilder
	{
		#region Methods

		public virtual IApplicationBuilder Use(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>()).Database.Migrate();
			}

			applicationBuilder.UseIdentityServerWsFederationPlugin();

			return applicationBuilder;
		}

		#endregion
	}
}