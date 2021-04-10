using System;
using System.Collections.Generic;
using System.Security.Claims;
using HansKindberg.IdentityServer.Extensions;
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

		/// <summary>
		/// The key can not contain colon, ":". Any colon should be url-encoded to "%3a". This is because keys can not contain colon, ":", in AppSettings.json.
		/// </summary>
		public virtual IDictionary<string, string> ClaimTypeMap { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ClaimTypes.Email.UrlEncodeColon(), JwtClaimTypes.Email},
			{"http%3a//schemas.microsoft.com/identity/claims/identityprovider", JwtClaimTypes.IdentityProvider},
			{ClaimTypes.Name.UrlEncodeColon(), JwtClaimTypes.Name},
			{ClaimTypes.NameIdentifier.UrlEncodeColon(), JwtClaimTypes.Subject},
			{ClaimTypes.PrimarySid.UrlEncodeColon(), "primarysid"},
			{ClaimTypes.SerialNumber.UrlEncodeColon(), "certserialnumber"},
			{"http%3a//schemas.microsoft.com/2012/12/certificatecontext/field/subject", "certsubject"},
			{ClaimTypes.Thumbprint.UrlEncodeColon(), "certthumbprint"},
			{ClaimTypes.Upn.UrlEncodeColon(), "upn"},
			{ClaimTypes.WindowsAccountName.UrlEncodeColon(), "winaccountname"},
			{ClaimTypes.X500DistinguishedName.UrlEncodeColon(), "certsubject"}
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