using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rsk.Saml;
using Rsk.Saml.Extensions;
using Rsk.Saml.Generators;
using Rsk.Saml.Validation;

namespace HansKindberg.IdentityServer.Saml.Generators
{
	public class Saml2SingleSignOnInteractionGenerator : ISaml2SingleSignOnInteractionGenerator
	{
		#region Constructors

		public Saml2SingleSignOnInteractionGenerator(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ExtendedIdentityServerOptions> identityServerOptionsMonitor, Rsk.Saml.DuendeIdentityServer.Generators.Saml2SingleSignOnInteractionGenerator internalSaml2SingleSignOnInteractionGenerator)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.IdentityServerOptionsMonitor = identityServerOptionsMonitor ?? throw new ArgumentNullException(nameof(identityServerOptionsMonitor));
			this.InternalSaml2SingleSignOnInteractionGenerator = internalSaml2SingleSignOnInteractionGenerator ?? throw new ArgumentNullException(nameof(internalSaml2SingleSignOnInteractionGenerator));
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServerOptionsMonitor { get; }
		protected internal virtual Rsk.Saml.DuendeIdentityServer.Generators.Saml2SingleSignOnInteractionGenerator InternalSaml2SingleSignOnInteractionGenerator { get; }

		#endregion

		#region Methods

		public virtual async Task<SamlInteractionResponse> ProcessInteraction(ValidatedSamlMessage request)
		{
			if(request == null)
				throw new ArgumentNullException(nameof(request));

			var samlInteractionResponse = await this.InternalSaml2SingleSignOnInteractionGenerator.ProcessInteraction(request);

			// ReSharper disable InvertIf
			if(!samlInteractionResponse.IsError && !samlInteractionResponse.IsLogin)
			{
				var path = this.HttpContextAccessor.HttpContext?.Request.Path;
				var samlIdpOptions = this.IdentityServerOptionsMonitor.CurrentValue.Saml;

				if(samlIdpOptions.ForceAuthenticationSupportEnabled && path != null && path == samlIdpOptions.SamlEndpoint.EnsureLeadingSlash().EnsureTrailingSlash() + SamlConstants.ProtocolRoutePaths.Saml2SingleSignOn)
				{
					var xml = XDocument.Parse(request.DecodedRawSamlMessage, LoadOptions.PreserveWhitespace).Root;
					var forceAuthenticationValue = xml?.Attribute("ForceAuthn")?.Value;

					if(forceAuthenticationValue != null && bool.TryParse(forceAuthenticationValue, out var forceAuthentication) && forceAuthentication)
						samlInteractionResponse = SamlInteractionResponse.Login();
				}
			}
			// ReSharper restore InvertIf

			return samlInteractionResponse;
		}

		#endregion
	}
}