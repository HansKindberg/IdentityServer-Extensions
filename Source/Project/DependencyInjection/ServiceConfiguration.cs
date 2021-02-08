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
	public class ServiceConfiguration : IServiceConfiguration
	{
		#region Constructors

		public ServiceConfiguration(IConfiguration configuration, IHostEnvironment hostEnvironment)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(hostEnvironment == null)
				throw new ArgumentNullException(nameof(hostEnvironment));

			var services = new ServiceCollection();

			services.AddFeatureManagement();
			services.AddHttpContextAccessor();
			services.AddSingleton(AppDomain.CurrentDomain);
			services.AddSingleton(configuration);
			services.AddSingleton<FileCertificateResolver>();
			services.AddSingleton(hostEnvironment);
			services.AddSingleton<IApplicationDomain, ApplicationHost>();
			services.AddSingleton<ICertificateResolver, CertificateResolver>();
			services.AddSingleton<StoreCertificateResolver>();
			services.Configure<DataOptions>(configuration.GetSection(ConfigurationKeys.DataPath));
			services.Configure<DevelopmentOptions>(configuration.GetSection(ConfigurationKeys.DevelopmentPath));

			var serviceProvider = services.BuildServiceProvider();

			this.ApplicationDomain = serviceProvider.GetRequiredService<IApplicationDomain>();
			this.CertificateResolver = serviceProvider.GetRequiredService<ICertificateResolver>();
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