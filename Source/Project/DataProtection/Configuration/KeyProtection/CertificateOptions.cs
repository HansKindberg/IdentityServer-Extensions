using System;
using HansKindberg.IdentityServer.DependencyInjection;
using HansKindberg.IdentityServer.DependencyInjection.Extensions;
using Microsoft.AspNetCore.DataProtection;
using RegionOrebroLan.Configuration;
using RegionOrebroLan.Security.Cryptography.Configuration;

namespace HansKindberg.IdentityServer.DataProtection.Configuration.KeyProtection
{
	public class CertificateOptions : KeyProtectionOptions
	{
		#region Properties

		public virtual DynamicOptions CertificateResolver { get; set; } = new DynamicOptions
		{
			Type = typeof(StoreResolverOptions).AssemblyQualifiedName
		};

		#endregion

		#region Methods

		public override void Add(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			dataProtectionBuilder.ProtectKeysWithCertificate(serviceConfiguration.GetCertificate(this.CertificateResolver));
		}

		#endregion
	}
}