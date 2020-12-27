using System;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace HansKindberg.IdentityServer.DataProtection.Configuration.KeyProtection
{
	public class DpapiOptions : KeyProtectionOptions
	{
		#region Properties

		public virtual bool ProtectToLocalMachine { get; set; }

		#endregion

		#region Methods

		public override void Add(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			dataProtectionBuilder.ProtectKeysWithDpapi(this.ProtectToLocalMachine);
		}

		#endregion
	}
}