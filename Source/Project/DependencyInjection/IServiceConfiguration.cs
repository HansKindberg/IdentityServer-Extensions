using HansKindberg.IdentityServer.Data.Configuration;
using HansKindberg.IdentityServer.Development.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan;
using RegionOrebroLan.DependencyInjection;
using RegionOrebroLan.Security.Cryptography;

namespace HansKindberg.IdentityServer.DependencyInjection
{
	public interface IServiceConfiguration
	{
		#region Properties

		IApplicationDomain ApplicationDomain { get; }
		ICertificateResolver CertificateResolver { get; }
		IConfiguration Configuration { get; }
		IOptions<DataOptions> Data { get; }
		IOptions<DevelopmentOptions> Development { get; }
		IFeatureManager FeatureManager { get; }
		IHostEnvironment HostEnvironment { get; }
		IInstanceFactory InstanceFactory { get; }

		#endregion
	}
}