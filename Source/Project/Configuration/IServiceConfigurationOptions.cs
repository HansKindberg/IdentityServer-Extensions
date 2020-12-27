using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Configuration
{
	public interface IServiceConfigurationOptions
	{
		#region Methods

		void Add(IServiceConfiguration serviceConfiguration, IServiceCollection services, params IConfigurationSection[] configurationSections);

		#endregion
	}
}