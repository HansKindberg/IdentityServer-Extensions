using System;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data.Transferring;
using HansKindberg.IdentityServer.Data.Transferring.Extensions;
using HansKindberg.IdentityServer.DataProtection.Configuration;
using HansKindberg.IdentityServer.Development;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity.Data;
using HansKindberg.IdentityServer.Logging.Configuration;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Web.Authentication.Builder.Extensions;

namespace HansKindberg.IdentityServer.Builder
{
	public static class ApplicationBuilderExtension
	{
		#region Methods

		private static IApplicationBuilder UseDatabaseContext<T>(this IApplicationBuilder applicationBuilder) where T : DbContext
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
			{
				serviceScope.ServiceProvider.GetRequiredService<T>().Database.Migrate();
			}

			return applicationBuilder;
		}

		public static IApplicationBuilder UseDataProtection(this IApplicationBuilder applicationBuilder)
		{
			return applicationBuilder.UseFeature<ExtendedDataProtectionOptions>();
		}

		public static IApplicationBuilder UseDataSeeding(this IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			using(var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
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
				.UseLoggingProvider()
				.UseStaticFiles()
				.UseRouting()
				.UseRequestLocalization()
				.ResolveWindowsAuthentication()
				.UseIdentityServer()
				.UseAuthorization();

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

		private static IApplicationBuilder UseFeature<T>(this IApplicationBuilder applicationBuilder) where T : IApplicationOptions
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			applicationBuilder.ApplicationServices.GetService<T>()?.Use(applicationBuilder);

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

			using(var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<IConfigurationDbContext>()).Database.Migrate();
				((DbContext)serviceScope.ServiceProvider.GetRequiredService<IPersistedGrantDbContext>()).Database.Migrate();
			}

			return applicationBuilder;
		}

		public static IApplicationBuilder UseLoggingProvider(this IApplicationBuilder applicationBuilder)
		{
			return applicationBuilder.UseFeature<LoggingProviderOptions>();
		}

		#endregion
	}
}