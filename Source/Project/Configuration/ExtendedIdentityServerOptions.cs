using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using RegionOrebroLan.Configuration;
using RegionOrebroLan.Security.Cryptography.Configuration;
using Rsk.Saml.Configuration;
using Rsk.WsFederation.Configuration;

namespace HansKindberg.IdentityServer.Configuration
{
	public class ExtendedIdentityServerOptions : IdentityServerOptions
	{
		#region Properties

		public virtual AccountOptions Account { get; set; } = new AccountOptions();

		public virtual IDictionary<string, string> ClaimTypeMap { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ClaimTypes.Email, JwtClaimTypes.Email},
			{"http://schemas.microsoft.com/identity/claims/identityprovider", JwtClaimTypes.IdentityProvider},
			{ClaimTypes.Name, JwtClaimTypes.Name},
			{ClaimTypes.NameIdentifier, JwtClaimTypes.Subject},
			{ClaimTypes.PrimarySid, "primarysid"},
			{ClaimTypes.SerialNumber, "certserialnumber"},
			{ClaimTypes.Thumbprint, "certthumbprint"},
			{ClaimTypes.Upn, "upn"},
			{ClaimTypes.WindowsAccountName, "winaccountname"}
		};

		public virtual ConfigurationStoreOptions ConfigurationStore { get; set; } = new ConfigurationStoreOptions();
		public virtual ConsentOptions Consent { get; set; } = new ConsentOptions();

		/// <summary>
		/// Like MutualTlsOptions.DomainName but used for interactive client authentication.
		/// </summary>
		public virtual string InteractiveMutualTlsDomain { get; set; }

		public virtual OperationalStoreOptions OperationalStore { get; set; } = new OperationalStoreOptions();
		public virtual RedirectionOptions Redirection { get; set; } = new RedirectionOptions();
		public virtual SamlIdpOptions Saml { get; set; } = new SamlIdpOptions();

		public virtual DynamicOptions SigningCertificate { get; set; } = new DynamicOptions
		{
			Options = new ConfigurationBuilder()
				.Add(new MemoryConfigurationSource
				{
					InitialData = new Dictionary<string, string>
					{
						{$"{ConfigurationKeys.IdentityServerPath}:{nameof(SigningCertificate)}:{nameof(DynamicOptions.Options)}:{nameof(StoreResolverOptions.Path)}", @"CERT:\LocalMachine\My\CN=Identity-Server-Signing"}
					}
				})
				.Build()
				.GetSection($"{ConfigurationKeys.IdentityServerPath}:{nameof(SigningCertificate)}:{nameof(DynamicOptions.Options)}"),
			Type = typeof(StoreResolverOptions).AssemblyQualifiedName
		};

		public virtual IList<DynamicOptions> ValidationCertificates { get; } = new List<DynamicOptions>();
		public virtual WsFederationOptions WsFederation { get; set; } = new WsFederationOptions();

		#endregion
	}
}