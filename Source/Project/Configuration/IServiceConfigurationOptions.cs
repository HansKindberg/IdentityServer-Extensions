using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Configuration
{
	public interface IServiceConfigurationOptions
	{
		#region Methods

		void Add(IServiceConfigurationBuilder serviceConfigurationBuilder, IServiceCollection services, params IConfigurationSection[] configurationSections);

		#endregion
	}
}