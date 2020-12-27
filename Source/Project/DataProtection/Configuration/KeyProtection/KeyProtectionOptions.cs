using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace HansKindberg.IdentityServer.DataProtection.Configuration.KeyProtection
{
	public abstract class KeyProtectionOptions
	{
		#region Methods

		public abstract void Add(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration);

		#endregion
	}
}