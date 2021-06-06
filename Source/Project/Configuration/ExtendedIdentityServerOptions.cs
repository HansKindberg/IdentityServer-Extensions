using System;
using System.Collections.Generic;
using System.Security.Claims;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.Options;
using HansKindberg.IdentityServer.Extensions;
using IdentityModel;
using Rsk.Saml.Configuration;
using Rsk.WsFederation.Configuration;

namespace HansKindberg.IdentityServer.Configuration
{
	public class ExtendedIdentityServerOptions : IdentityServerOptions
	{
		#region Properties

		public virtual AccountOptions Account { get; set; } = new AccountOptions();

		/// <summary>
		/// The key can not contain colon, ":". Any colon should be url-encoded to "%3a". This is because keys can not contain colon, ":", in appsettings.json.
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
		public virtual WsFederationOptions WsFederation { get; set; } = new WsFederationOptions();

		#endregion
	}
}