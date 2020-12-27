using System;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace HansKindberg.IdentityServer.DataProtection.Configuration.KeyProtection
{
	public class DpapiNgOptions : KeyProtectionOptions
	{
		#region Properties

		public virtual DpapiNGProtectionDescriptorFlags Flags { get; set; }
		public virtual string ProtectionDescriptorRule { get; set; }

		#endregion

		#region Methods

		public override void Add(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			if(this.ProtectionDescriptorRule == null)
				dataProtectionBuilder.ProtectKeysWithDpapiNG();
			else
				dataProtectionBuilder.ProtectKeysWithDpapiNG(this.ProtectionDescriptorRule, this.Flags);
		}

		#endregion
	}
}