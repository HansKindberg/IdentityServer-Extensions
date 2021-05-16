using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rsk.Saml.Configuration;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;

namespace HansKindberg.IdentityServer.Builder
{
	public class SamlPluginBuilder : ISamlPluginBuilder
	{
		#region Methods

		public virtual IApplicationBuilder Use(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>()).Database.Migrate();
			}

			applicationBuilder.UseIdentityServerSamlPlugin();

			return applicationBuilder;
		}

		#endregion
	}
}