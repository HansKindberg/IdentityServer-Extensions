using System;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Rsk.Saml;
using Rsk.Saml.Extensions;
using Rsk.Saml.Generators;
using Rsk.Saml.Models;
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

		protected internal virtual bool ForceAuthenticationIsAllowed
		{
			get
			{
				var samlIdpOptions = this.IdentityServerOptionsMonitor.CurrentValue.Saml;

				if(!samlIdpOptions.ForceAuthentication.Enabled)
					return false;

				var path = this.HttpContextAccessor.HttpContext?.Request.Path;

				if(path == null)
					return false;

				// ReSharper disable ConvertIfStatementToReturnStatement
				if(path != samlIdpOptions.SamlEndpoint.EnsureLeadingSlash().EnsureTrailingSlash() + SamlConstants.ProtocolRoutePaths.Saml2SingleSignOn)
					return false;
				// ReSharper restore ConvertIfStatementToReturnStatement

				return true;
			}
		}

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
				if(this.ForceAuthenticationIsAllowed && request.Message is Saml2Request { ForceAuthentication: true })
					samlInteractionResponse = SamlInteractionResponse.Login();
			}
			// ReSharper restore InvertIf

			return samlInteractionResponse;
		}

		#endregion
	}
}