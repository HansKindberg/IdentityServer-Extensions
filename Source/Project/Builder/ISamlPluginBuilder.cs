using Microsoft.AspNetCore.Builder;

namespace HansKindberg.IdentityServer.Builder
{
	public interface ISamlPluginBuilder
	{
		#region Methods

		IApplicationBuilder Use(IApplicationBuilder applicationBuilder);

		#endregion
	}
}