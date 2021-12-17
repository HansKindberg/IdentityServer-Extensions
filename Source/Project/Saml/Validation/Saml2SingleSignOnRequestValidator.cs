using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Rsk.Saml.Models;
using Rsk.Saml.Validation;

namespace HansKindberg.IdentityServer.Saml.Validation
{
	public class Saml2SingleSignOnRequestValidator : ISaml2SingleSignOnRequestValidator
	{
		#region Constructors

		public Saml2SingleSignOnRequestValidator(Rsk.Saml.Validation.Saml2SingleSignOnRequestValidator internalSaml2SingleSignOnRequestValidator)
		{
			this.InternalSaml2SingleSignOnRequestValidator = internalSaml2SingleSignOnRequestValidator ?? throw new ArgumentNullException(nameof(internalSaml2SingleSignOnRequestValidator));
		}

		#endregion

		#region Properties

		protected internal virtual Rsk.Saml.Validation.Saml2SingleSignOnRequestValidator InternalSaml2SingleSignOnRequestValidator { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<SamlValidationResult> Validate(NameValueCollection parameters, string bindingType, string rawUrl, string baseUrl, string issuerUri, ClaimsPrincipal subject = null)
		{
			var samlValidationResult = await this.InternalSaml2SingleSignOnRequestValidator.Validate(parameters, bindingType, rawUrl, baseUrl, issuerUri, subject);

			// ReSharper disable InvertIf
			if(!samlValidationResult.IsError && samlValidationResult.ValidatedMessage.Message is Saml2Request saml2Request)
			{
				var xml = XDocument.Parse(samlValidationResult.ValidatedMessage.DecodedRawSamlMessage, LoadOptions.PreserveWhitespace).Root;
				var forceAuthenticationValue = xml?.Attribute("ForceAuthn")?.Value;

				if(forceAuthenticationValue != null && bool.TryParse(forceAuthenticationValue, out var forceAuthentication))
					saml2Request.ForceAuthentication = forceAuthentication;
			}
			// ReSharper restore InvertIf

			return samlValidationResult;
		}

		#endregion
	}
}