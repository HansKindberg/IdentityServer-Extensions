using Microsoft.AspNetCore.Builder;

namespace HansKindberg.IdentityServer.Configuration
{
	public interface IApplicationOptions
	{
		#region Methods

		void Use(IApplicationBuilder applicationBuilder);

		#endregion
	}
}