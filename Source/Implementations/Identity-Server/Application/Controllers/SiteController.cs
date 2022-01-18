using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Application.Models.Views.Shared;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Web;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;
using Rsk.Saml.DuendeIdentityServer.Services.Models;
using Rsk.Saml.Models;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public abstract class SiteController : Controller
	{
		#region Constructors

		protected SiteController(IFacade facade)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
			this.Localizer = (facade.LocalizerFactory ?? throw new ArgumentException("The localizer-factory property can not be null.", nameof(facade))).Create(this.GetType());
			this.Logger = (facade.LoggerFactory ?? throw new ArgumentException("The logger-factory property can not be null.", nameof(facade))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IFacade Facade { get; }
		protected internal virtual IStringLocalizer Localizer { get; }
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<T> CreateSingleSignOutViewModelAsync<T>(string signOutId) where T : SingleSignOutViewModel, new()
		{
			var signOutOptions = this.Facade.IdentityServer.CurrentValue.SignOut;

			var includeSignOutIframe = await this.IncludeSignOutIframeAsync(signOutId, signOutOptions.Mode);

			if(signOutOptions.Mode.HasFlag(SingleSignOutMode.IdpInitiated))
				signOutId ??= await this.Facade.Interaction.CreateLogoutContextAsync();

			var signOutRequest = await this.Facade.Interaction.GetLogoutContextAsync(signOutId);

			var model = new T
			{
				AutomaticRedirect = signOutOptions.AutomaticRedirectAfterSignOut,
				Client = string.IsNullOrEmpty(signOutRequest?.ClientName) ? signOutRequest?.ClientId : signOutRequest.ClientName,
				IframeUrl = includeSignOutIframe ? signOutRequest?.SignOutIFrameUrl : null,
				RedirectUrl = signOutRequest?.PostLogoutRedirectUri,
				SecondsBeforeRedirect = signOutOptions.SecondsBeforeRedirectAfterSignOut
			};

			// ReSharper disable InvertIf
			if(this.Facade.FeatureManager.IsEnabled(Feature.Saml))
			{
				if(includeSignOutIframe && signOutRequest != null)
				{
					var samlSignOutRequest = new SamlLogoutRequest(signOutRequest);

					if(samlSignOutRequest.ServiceProviderIds != null)
						model.SamlIframeUrl = await this.Facade.SamlInteraction.GetSamlSignOutFrameUrl(signOutId, samlSignOutRequest);
				}

				var samlRequestId = await this.GetSamlRequestIdAsync();

				if(samlRequestId != null)
				{
					var redirectUrl = await this.Facade.SamlInteraction.GetLogoutCompletionUrl(samlRequestId);

					if(redirectUrl != null)
						model.RedirectUrl = redirectUrl;
				}
			}
			// ReSharper restore InvertIf

			return model;
		}

		protected internal virtual string GetLocalizedValue(string key, params string[] argumentKeys)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			var arguments = new List<object>();

			// ReSharper disable LoopCanBeConvertedToQuery
			foreach(var argumentKey in argumentKeys ?? Array.Empty<string>())
			{
				var localizedArgument = this.Localizer.GetString(argumentKey);

				arguments.Add(localizedArgument.ResourceNotFound ? argumentKey : localizedArgument);
			}
			// ReSharper restore LoopCanBeConvertedToQuery

			return this.Localizer.GetString(key, arguments.ToArray());
		}

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		protected internal virtual async Task<string> GetSamlRequestIdAsync()
		{
			var requestIdParameter = this.Facade.IdentityServer.CurrentValue.Saml.UserInteraction.RequestIdParameter;

			var requestId = (string)this.HttpContext.Request.Query[requestIdParameter];

			// ReSharper disable InvertIf
			if(requestId == null && await this.IsSamlForceAuthenticationRequestAsync())
			{
				// If the Saml-feature is enabled and the request is a force-authentication request we look after the saml-request-id in the return-url parameter.
				var url = this.HttpContext.Request.Query.GetValueAsAbsoluteUrl(QueryStringKeys.ReturnUrl);

				if(url != null)
				{
					var query = QueryHelpers.ParseQuery(url.Query);

					if(query.ContainsKey(requestIdParameter))
						requestId = query[requestIdParameter];
				}
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(requestId);
		}

		protected internal virtual async Task<bool> IncludeSignOutIframeAsync(string signOutId, SingleSignOutMode singleSignOutMode)
		{
			if(singleSignOutMode == SingleSignOutMode.None)
				return false;

			if(signOutId != null && singleSignOutMode.HasFlag(SingleSignOutMode.ClientInitiated))
				return true;

			if(signOutId == null && singleSignOutMode.HasFlag(SingleSignOutMode.IdpInitiated))
				return true;

			return await Task.FromResult(false);
		}

		protected internal virtual async Task<bool> IsSamlForceAuthenticationRequestAsync()
		{
			if(!this.Facade.FeatureManager.IsEnabled(Feature.Saml))
				return false;

			var returnUrl = (string)this.HttpContext.Request.Query[QueryStringKeys.ReturnUrl];

			if(string.IsNullOrWhiteSpace(returnUrl))
				return false;

			var validatedSamlMessage = await this.Facade.SamlInteraction.GetRequestContext(returnUrl);

			return validatedSamlMessage.Message is Saml2Request { ForceAuthentication: true };
		}

		protected internal virtual async Task<IActionResult> Redirect(string redirectUrl, byte secondsBeforeRedirect)
		{
			this.HttpContext.Response.Headers["Location"] = string.Empty;
			this.HttpContext.Response.StatusCode = 200;

			return await Task.FromResult(this.View("Redirect", new RedirectViewModel { RedirectUrl = redirectUrl, SecondsBeforeRedirect = secondsBeforeRedirect }));
		}

		protected internal virtual string ResolveAndValidateReturnUrl(string returnUrl)
		{
			returnUrl = this.ResolveReturnUrl(returnUrl);

			// ReSharper disable InvertIf
			if(!this.Url.IsLocalUrl(returnUrl) && !this.Facade.Interaction.IsValidReturnUrl(returnUrl))
			{
				var message = $"The return-url \"{returnUrl}\" is invalid";

				this.Logger.LogErrorIfEnabled(message);

				throw new InvalidOperationException(message);
			}
			// ReSharper restore InvertIf

			return returnUrl;
		}

		protected internal virtual string ResolveReturnUrl(string returnUrl)
		{
			return string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl;
		}

		#endregion
	}
}