using Microsoft.AspNetCore.Builder;

namespace HansKindberg.IdentityServer.Builder
{
	public interface IWsFederationPluginBuilder
	{
		#region Methods

		IApplicationBuilder Use(IApplicationBuilder applicationBuilder);

		#endregion
	}
}