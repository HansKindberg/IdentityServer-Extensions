using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Rsk.Saml.Models;
using Rsk.Saml.Services;
using Rsk.Saml.Validation;

namespace HansKindberg.IdentityServer.Saml.Services
{
	public class SamlPersistedGrantService : ISamlPersistedGrantService
	{
		#region Constructors

		public SamlPersistedGrantService(Rsk.Saml.Services.SamlPersistedGrantService internalSamlPersistedGrantService)
		{
			this.InternalSamlPersistedGrantService = internalSamlPersistedGrantService ?? throw new ArgumentNullException(nameof(internalSamlPersistedGrantService));
		}

		#endregion

		#region Properties

		protected internal virtual Rsk.Saml.Services.SamlPersistedGrantService InternalSamlPersistedGrantService { get; }

		#endregion

		#region Methods

		public virtual async Task<ValidatedSamlMessage> GetRequest(string key)
		{
			var validatedSamlMessage = await this.InternalSamlPersistedGrantService.GetRequest(key);

			// ReSharper disable InvertIf
			if(validatedSamlMessage.Message is Saml2Request saml2Request)
			{
				var forceAuthenticationValue = validatedSamlMessage.Raw[nameof(Saml2Request.ForceAuthentication)];

				if(forceAuthenticationValue != null && bool.TryParse(forceAuthenticationValue, out var forceAuthentication))
					saml2Request.ForceAuthentication = forceAuthentication;
			}
			// ReSharper restore InvertIf

			return validatedSamlMessage;
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		public virtual async Task<string> StoreRequest(ValidatedSamlMessage request)
		{
			if(request == null)
				throw new ArgumentNullException(nameof(request));

			if(request.Message is Saml2Request { ForceAuthentication: true } saml2Request)
				request.Raw[nameof(Saml2Request.ForceAuthentication)] = saml2Request.ForceAuthentication.ToString().ToLowerInvariant();

			return await this.InternalSamlPersistedGrantService.StoreRequest(request);
		}

		#endregion
	}
}