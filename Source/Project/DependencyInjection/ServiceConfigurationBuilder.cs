using System;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Data.Configuration;
using HansKindberg.IdentityServer.Development.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Security.Cryptography;

namespace HansKindberg.IdentityServer.DependencyInjection
{
	public class ServiceConfigurationBuilder : IServiceConfigurationBuilder
	{
		#region Constructors

		public ServiceConfigurationBuilder(IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			var services = new ServiceCollection();

			services.AddFeatureManagement();
			services.AddHttpContextAccessor();
			services.AddSingleton(configuration);
			services.AddSingleton(hostEnvironment);
			services.Configure<DataOptions>(configuration.GetSection(ConfigurationKeys.DataPath));
			services.Configure<DevelopmentOptions>(configuration.GetSection(ConfigurationKeys.DevelopmentPath));

			var serviceProvider = services.BuildServiceProvider();

			var applicationDomain = new ApplicationHost(AppDomain.CurrentDomain, hostEnvironment);
			this.ApplicationDomain = applicationDomain;

			var fileCertificateResolver = new FileCertificateResolver(applicationDomain);
			var storeCertificateResolver = new StoreCertificateResolver();
			this.CertificateResolver = new CertificateResolver(fileCertificateResolver, storeCertificateResolver);

			this.Configuration = configuration;
			this.Data = serviceProvider.GetRequiredService<IOptions<DataOptions>>();
			this.Development = serviceProvider.GetRequiredService<IOptions<DevelopmentOptions>>();
			this.FeatureManager = serviceProvider.GetRequiredService<IFeatureManager>();
			this.HostEnvironment = hostEnvironment;
			this.InstanceFactory = new InstanceFactory();
		}

		#endregion

		#region Properties

		public virtual IApplicationDomain ApplicationDomain { get; }
		public virtual ICertificateResolver CertificateResolver { get; }
		public virtual IConfiguration Configuration { get; }
		public virtual IOptions<DataOptions> Data { get; }
		public virtual IOptions<DevelopmentOptions> Development { get; }
		public virtual IFeatureManager FeatureManager { get; }
		public virtual IHostEnvironment HostEnvironment { get; }
		public virtual IInstanceFactory InstanceFactory { get; }

		#endregion
	}
}