using System;
using System.Diagnostics.CodeAnalysis;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data.Configuration;
using HansKindberg.IdentityServer.Development.Configuration;
using HansKindberg.IdentityServer.FeatureManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using RegionOrebroLan;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Security.Cryptography;
using Serilog;

namespace HansKindberg.IdentityServer.DependencyInjection
{
	public class ServiceConfigurationBuilder : IServiceConfigurationBuilder
	{
		#region Constructors

		[SuppressMessage("Style", "IDE0016:Use 'throw' expression")]
		public ServiceConfigurationBuilder(IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			var applicationDomain = new ApplicationHost(AppDomain.CurrentDomain, hostEnvironment);
			this.ApplicationDomain = applicationDomain;

			var fileCertificateResolver = new FileCertificateResolver(applicationDomain);
			var storeCertificateResolver = new StoreCertificateResolver();
			this.CertificateResolver = new CertificateResolver(fileCertificateResolver, storeCertificateResolver);

			this.Configuration = configuration;

			var data = new DataOptions();
			configuration.GetSection(ConfigurationKeys.DataPath).Bind(data);
			this.Data = data;

			var development = new DevelopmentOptions();
			configuration.GetSection(ConfigurationKeys.DevelopmentPath).Bind(development);
			this.Development = development;

			this.FeatureManager = new ConfigurationFeatureManager(configuration);
			this.HostEnvironment = hostEnvironment;
			this.InstanceFactory = new InstanceFactory();
		}

		#endregion

		#region Properties

		public virtual IApplicationDomain ApplicationDomain { get; }
		public virtual ICertificateResolver CertificateResolver { get; }
		public virtual IConfiguration Configuration { get; }
		public virtual DataOptions Data { get; }
		public virtual DevelopmentOptions Development { get; }
		public virtual IFeatureManager FeatureManager { get; }
		public virtual IHostEnvironment HostEnvironment { get; }
		public virtual IInstanceFactory InstanceFactory { get; }
		public virtual ILogger Logger => Log.Logger;

		#endregion
	}
}