using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.Development
{
	public interface IDevelopmentStartup
	{
		#region Methods

		void Configure(IApplicationBuilder applicationBuilder);
		void ConfigureServices(IServiceCollection services);

		#endregion
	}
}