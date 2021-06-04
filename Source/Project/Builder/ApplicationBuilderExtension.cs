using System;
using System.Linq;
using Duende.IdentityServer.EntityFramework.Interfaces;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Extensions;
using HansKindberg.IdentityServer.Development;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.Web.Authorization.Builder.Extentsions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Caching.Distributed.Builder.Extensions;
using RegionOrebroLan.DataProtection.Builder.Extensions;
using RegionOrebroLan.Web.Authentication;

namespace HansKindberg.IdentityServer.Builder
{
	public static class ApplicationBuilderExtension
	{
		#region Methods

		public static IApplicationBuilder ResolveWindowsAuthentication(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			var windowsAuthenticationScheme = applicationBuilder.ApplicationServices.GetRequiredService<IAuthenticationSchemeLoader>().ListAsync().Result.FirstOrDefault(authenticationScheme => string.Equals(authenticationScheme.Name, IISServerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase));

			// ReSharper disable MergeIntoNegatedPattern

			if(windowsAuthenticationScheme == null || !windowsAuthenticationScheme.Enabled)
				applicationBuilder.ApplicationServices.GetRequiredService<IAuthenticationSchemeProvider>().RemoveScheme(IISServerDefaults.AuthenticationScheme);

			// ReSharper restore MergeIntoNegatedPattern

			return applicationBuilder;
		}

		private static IApplicationBuilder UseDatabaseContext<T>(this IApplicationBuilder applicationBuilder) where T : DbContext
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<T>().Database.Migrate();
			}

			return applicationBuilder;
		}

		public static IApplicationBuilder UseDataSeeding(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<IDataImporter>().Import(applicationBuilder.ApplicationServices.GetRequiredService<IConfiguration>());
			}

			return applicationBuilder;
		}

		public static IApplicationBuilder UseDefault(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<Configuration.Options>>().Value;

			if(!options.Enabled)
			{
				applicationBuilder.Run(async context =>
				{
					await context.Response.WriteAsync("The application is not enabled. Configure it and enable it.");
				});

				return applicationBuilder;
			}

			var exceptionHandling = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<ExceptionHandlingOptions>>().Value;
			if(exceptionHandling.DeveloperExceptionPage)
				applicationBuilder.UseDeveloperExceptionPage();
			else
				applicationBuilder.UseExceptionHandler(exceptionHandling.Path);

			var featureManager = applicationBuilder.ApplicationServices.GetRequiredService<IFeatureManager>();

			if(featureManager.IsEnabled(Feature.CertificateForwarding))
				applicationBuilder.UseCertificateForwarding();

			if(featureManager.IsEnabled(Feature.ForwardedHeaders))
				applicationBuilder.UseForwardedHeaders();

			if(featureManager.IsEnabled(Feature.Development))
				applicationBuilder.UseDevelopment();

			if(featureManager.IsEnabled(Feature.Hsts))
				applicationBuilder.UseHsts();

			if(featureManager.IsEnabled(Feature.HttpsRedirection))
				applicationBuilder.UseHttpsRedirection();

			applicationBuilder.UseIdentity();

			applicationBuilder
				.UseCookiePolicy()
				.UseDataProtection()
				.UseDistributedCache()
				.UseStaticFiles()
				.UseRouting()
				.UseRequestLocalization()
				.ResolveWindowsAuthentication()
				.UseIdentityServer()
				.UseExtendedAuthorization();

			if(featureManager.IsEnabled(Feature.DataSeeding))
				applicationBuilder.UseDataSeeding();

			applicationBuilder.UseEndpoints(endpointRouteBuilder => endpointRouteBuilder.MapDefaultControllerRoute());

			return applicationBuilder;
		}

		public static IApplicationBuilder UseDevelopment(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			applicationBuilder.ApplicationServices.GetRequiredService<IDevelopmentStartup>().Configure(applicationBuilder);

			return applicationBuilder;
		}

		public static IApplicationBuilder UseIdentity(this IApplicationBuilder applicationBuilder)
		{
			return applicationBuilder.UseDatabaseContext<IdentityContext>();
		}

		public static IApplicationBuilder UseIdentityServer(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			applicationBuilder.UseIdentityServer(new IdentityServerMiddlewareOptions());

			using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
			{
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>()).Database.Migrate();
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<IPersistedGrantDbContext>()).Database.Migrate();
			}

			var featureManager = applicationBuilder.ApplicationServices.GetRequiredService<IFeatureManager>();

			if(featureManager.IsEnabled(Feature.Saml))
				applicationBuilder.ApplicationServices.GetRequiredService<ISamlPluginBuilder>().Use(applicationBuilder);

			if(featureManager.IsEnabled(Feature.WsFederation))
				applicationBuilder.ApplicationServices.GetRequiredService<IWsFederationPluginBuilder>().Use(applicationBuilder);

			return applicationBuilder;
		}

		#endregion
	}
}