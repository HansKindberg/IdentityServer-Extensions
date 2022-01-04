using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Saml.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rsk.Saml.Generators;
using Rsk.Saml.Models;
using Rsk.Saml.Services;
using Rsk.Saml.Validation;

namespace HansKindberg.IdentityServer.Saml.Services
{
	[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
	public class ExtendedSamlInteractionService : IExtendedSamlInteractionService
	{
		#region Constructors

		public ExtendedSamlInteractionService(IForceAuthenticationRouter forceAuthenticationRouter, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ExtendedIdentityServerOptions> identityServer, DefaultSamlInteractionService internalSamlInteractionService)
		{
			this.ForceAuthenticationRouter = forceAuthenticationRouter ?? throw new ArgumentNullException(nameof(forceAuthenticationRouter));
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.IdentityServer = identityServer ?? throw new ArgumentNullException(nameof(identityServer));
			this.InternalSamlInteractionService = internalSamlInteractionService ?? throw new ArgumentNullException(nameof(internalSamlInteractionService));
		}

		#endregion

		#region Properties

		protected internal virtual IForceAuthenticationRouter ForceAuthenticationRouter { get; }
		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServer { get; }
		protected internal virtual DefaultSamlInteractionService InternalSamlInteractionService { get; }

		#endregion

		#region Methods

		public virtual async Task<GeneratedMessage> CreateIdpInitiatedSsoResponse(string serviceProviderId)
		{
			return await this.InternalSamlInteractionService.CreateIdpInitiatedSsoResponse(serviceProviderId);
		}

		public virtual async Task ExecuteIdpInitiatedSso(HttpContext context, GeneratedMessage response)
		{
			await this.InternalSamlInteractionService.ExecuteIdpInitiatedSso(context, response);
		}

		public virtual async Task<IActionResult> GetForceAuthenticationActionResultAsync(string returnUrl)
		{
			var httpContext = this.HttpContextAccessor.HttpContext;
			if(httpContext == null || !httpContext.User.IsAuthenticated())
				return null;

			var samlIdpOptions = this.IdentityServer.CurrentValue.Saml;
			if(!samlIdpOptions.ForceAuthentication.Enabled)
				return null;

			var validatedSamlMessage = await this.GetRequestContext(returnUrl);

			if(validatedSamlMessage?.Message is Saml2Request { ForceAuthentication: true })
				return await this.ForceAuthenticationRouter.GetActionResultAsync(returnUrl);

			return null;
		}

		public virtual async Task<IEnumerable<ServiceProvider>> GetIdpInitiatedSsoCompatibleServiceProviders()
		{
			return await this.InternalSamlInteractionService.GetIdpInitiatedSsoCompatibleServiceProviders();
		}

		public virtual async Task<string> GetLogoutCompletionUrl(string logoutId)
		{
			return await this.InternalSamlInteractionService.GetLogoutCompletionUrl(logoutId);
		}

		public virtual async Task<ValidatedSamlMessage> GetLogoutContext(string logoutId)
		{
			return await this.InternalSamlInteractionService.GetLogoutContext(logoutId);
		}

		public virtual async Task<ValidatedSamlMessage> GetRequestContext(string returnUrl)
		{
			return await this.InternalSamlInteractionService.GetRequestContext(returnUrl);
		}

		public virtual async Task<string> GetSamlSignOutFrameUrl(string logoutId, ISamlLogoutRequest request)
		{
			return await this.InternalSamlInteractionService.GetSamlSignOutFrameUrl(logoutId, request);
		}

		#endregion
	}
}