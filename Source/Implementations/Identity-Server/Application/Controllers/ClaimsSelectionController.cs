using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Web.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using RegionOrebroLan.Collections.Generic.Extensions;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;
using Rsk.Saml.Models;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	[FeatureGate(Feature.ClaimsSelection)]
	public class ClaimsSelectionController : AuthenticateControllerBase
	{
		#region Fields

		private AuthenticateResult _authenticateResult;
		private Lazy<string> _authenticationScheme;
		private Lazy<IClaimsSelectionContext> _claimsSelectionContext;
		private const string _clientAuthenticationKey = "client";
		private const string _iframeUrlAuthenticationKey = "iframeUrl";
		private const string _samlIframeUrlAuthenticationKey = "samlIframeUrl";

		#endregion

		#region Constructors

		public ClaimsSelectionController(IFacade facade) : base(facade) { }

		#endregion

		#region Properties

		protected internal virtual AuthenticateResult AuthenticateResult
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._authenticateResult == null)
				{
					var authenticateResult = (this.User.IsAuthenticated() ? this.HttpContext.AuthenticateAsync().Result : this.HttpContext.AuthenticateAsync(this.IntermediateAuthenticationScheme).Result).Clone();

					if(this.User.IsAuthenticated())
					{
						var identityProviderClaim = this.User.Claims.FindFirstIdentityProviderClaim();

						if(identityProviderClaim != null)
							authenticateResult.Properties?.SetString(AuthenticationKeys.Scheme, identityProviderClaim.Value);
					}

					this._authenticateResult = authenticateResult;
				}
				// ReSharper restore InvertIf

				return this._authenticateResult;
			}
		}

		protected internal virtual string AuthenticationScheme
		{
			get
			{
				this._authenticationScheme ??= new Lazy<string>(() => this.AuthenticateResult.Succeeded ? this.AuthenticateResult.Properties?.GetString(AuthenticationKeys.Scheme) : null);

				return this._authenticationScheme.Value;
			}
		}

		protected internal virtual IClaimsSelectionContext ClaimsSelectionContext
		{
			get
			{
				this._claimsSelectionContext ??= new Lazy<IClaimsSelectionContext>(() => this.Facade.ClaimsSelectionContextAccessor.GetAsync(this.AuthenticationScheme, string.Empty).Result);

				return this._claimsSelectionContext.Value;
			}
		}

		protected internal virtual bool ClaimsSelectionFeaterEnabled => this.Facade.FeatureManager.IsEnabled(Feature.ClaimsSelection);
		protected internal virtual string ClientAuthenticationKey => _clientAuthenticationKey;
		protected internal virtual string IframeUrlAuthenticationKey => _iframeUrlAuthenticationKey;
		protected internal virtual string IntermediateAuthenticationScheme => this.IdentityServer.IntermediateCookieAuthenticationHandlers.ClaimsSelection.Name;
		protected internal virtual string SamlIframeUrlAuthenticationKey => _samlIframeUrlAuthenticationKey;

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Confirmation()
		{
			var authenticateResult = await this.HttpContext.AuthenticateAsync(this.IntermediateAuthenticationScheme);

			if(!authenticateResult.Succeeded)
			{
				this.Logger.LogErrorIfEnabled($"An authentication-attempt, using the scheme {this.IntermediateAuthenticationScheme.ToStringRepresentation()}, was made but failed. Available request-cookies: {string.Join(", ", this.Request.Cookies.Keys)}");

				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);
			}

			var model = await this.CreateClaimsSelectionConfirmationViewModelAsync(authenticateResult.Properties);

			await this.CallbackInternal(authenticateResult, this.IntermediateAuthenticationScheme);

			return this.View("Confirmation", model);
		}

		protected internal virtual async Task<AuthenticationProperties> CreateAuthenticationPropertiesAsync(string returnUrl)
		{
			var model = await this.CreateClaimsSelectionConfirmationViewModelAsync(returnUrl);

			var authenticationProperties = this.AuthenticateResult.Properties?.Clone() ?? new AuthenticationProperties();

			authenticationProperties.ExpiresUtc = null;
			authenticationProperties.IssuedUtc = null;

			authenticationProperties.RedirectUri = this.Url.Action(nameof(this.Confirmation));

			authenticationProperties.SetString(AuthenticationKeys.ClaimsSelectionHandled, true.ToString());
			authenticationProperties.SetString(this.ClientAuthenticationKey, model.Client);
			authenticationProperties.SetString(this.IframeUrlAuthenticationKey, model.IframeUrl);
			authenticationProperties.SetString(AuthenticationKeys.ReturnUrl, model.RedirectUrl);
			authenticationProperties.SetString(this.SamlIframeUrlAuthenticationKey, model.SamlIframeUrl);
			authenticationProperties.SetString(AuthenticationKeys.Scheme, this.AuthenticationScheme);

			return authenticationProperties;
		}

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(IEnumerable<IClaimsSelectionResult> claimsSelectionResults)
		{
			try
			{
				if(claimsSelectionResults == null)
					throw new ArgumentNullException(nameof(claimsSelectionResults));

				var claimsPrincipal = this.AuthenticateResult.Principal;

				if(claimsPrincipal == null)
					throw new InvalidOperationException("The claims-principal from the current authentication is null.");

				var claims = new ClaimBuilderCollection();
				claims.AddRange(claimsPrincipal.Claims.Select(claim => new ClaimBuilder(claim)));

				foreach(var result in claimsSelectionResults)
				{
					var selectedClaims = await result.Selector.GetClaimsAsync(claimsPrincipal, result);

					foreach(var selectedClaimType in selectedClaims.Keys)
					{
						for(var i = claims.Count - 1; i >= 0; i--)
						{
							if(string.Equals(claims[i].Type, selectedClaimType, StringComparison.OrdinalIgnoreCase))
								claims.RemoveAt(i);
						}
					}

					foreach(var claimBuilderCollection in selectedClaims.Values)
					{
						claims.AddRange(claimBuilderCollection);
					}
				}

				return await this.CreateClaimsPrincipalAsync(this.AuthenticateResult.Principal?.Identity?.AuthenticationType, claims);
			}
			catch(Exception exception)
			{
				throw new InvalidOperationException("Could not create claims-principal from claims-selection-results.", exception);
			}
		}

		protected internal virtual async Task<ClaimsSelectionConfirmationViewModel> CreateClaimsSelectionConfirmationViewModelAsync(string returnUrl)
		{
			var signOutId = await this.GetSignOutIdAsync(returnUrl);

			var model = await this.CreateSingleSignOutViewModelAsync<ClaimsSelectionConfirmationViewModel>(signOutId);

			model.AuthenticationScheme = this.AuthenticationScheme;
			model.RedirectUrl = returnUrl;

			return model;
		}

		protected internal virtual async Task<ClaimsSelectionConfirmationViewModel> CreateClaimsSelectionConfirmationViewModelAsync(AuthenticationProperties authenticationProperties)
		{
			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			var signOutOptions = this.Facade.IdentityServer.CurrentValue.SignOut;

			var model = new ClaimsSelectionConfirmationViewModel
			{
				AuthenticationScheme = authenticationProperties.GetString(AuthenticationKeys.Scheme),
				AutomaticRedirect = signOutOptions.AutomaticRedirectAfterSignOut,
				Client = authenticationProperties.GetString(this.ClientAuthenticationKey),
				IframeUrl = authenticationProperties.GetString(this.IframeUrlAuthenticationKey),
				RedirectUrl = authenticationProperties.GetString(AuthenticationKeys.ReturnUrl),
				SamlIframeUrl = authenticationProperties.GetString(this.SamlIframeUrlAuthenticationKey),
				SecondsBeforeRedirect = signOutOptions.SecondsBeforeRedirectAfterSignOut
			};

			return await Task.FromResult(model);
		}

		[SuppressMessage("Style", "IDE0057:Use range operator")]
		protected internal virtual async Task<ClaimsSelectionViewModel> CreateClaimsSelectionViewModelAsync(string returnUrl)
		{
			if(returnUrl == null)
				throw new ArgumentNullException(nameof(returnUrl));

			var model = new ClaimsSelectionViewModel
			{
				Form =
				{
					AuthenticationScheme = this.AuthenticationScheme
				},
				ReturnUrl = returnUrl
			};

			foreach(var claimsSelector in this.ClaimsSelectionContext.Selectors)
			{
				// ReSharper disable PossibleNullReferenceException
				var inputPrefix = $"{await this.GetFullTypeNameWithUnderscoresInsteadOfDotsAsync(claimsSelector)}.";
				// ReSharper restore PossibleNullReferenceException

				var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				if(HttpMethods.IsPost(this.HttpContext.Request.Method))
				{
					foreach(var (key, value) in this.HttpContext.Request.Form)
					{
						if(key.StartsWith(inputPrefix, StringComparison.OrdinalIgnoreCase))
							selections.Add(key.Substring(inputPrefix.Length), value);
					}
				}

				var result = await claimsSelector.SelectAsync(this.AuthenticateResult.Principal, selections);

				model.Results.Add(result);

				foreach(var (key, value) in result.Selectables)
				{
					var group = new ClaimsSelectionGroup { SelectionRequired = claimsSelector.SelectionRequired };
					group.SelectableClaims.Add(value);

					model.Form.Groups.Add($"{inputPrefix}{key}", group);
				}
			}

			model.Form.RequiredSelectionsSelected = !model.Form.Groups.Values.Any(group => group.SelectionRequired && !group.SelectableClaims.Any(selectableClaim => selectableClaim.Selected));

			return await Task.FromResult(model);
		}

		protected internal virtual async Task<string> GetFullTypeNameWithUnderscoresInsteadOfDotsAsync(object instance)
		{
			if(instance == null)
				throw new ArgumentNullException(nameof(instance));

			// ReSharper disable PossibleNullReferenceException
			return await Task.FromResult(instance.GetType().FullName.Replace('.', '_'));
			// ReSharper restore PossibleNullReferenceException
		}

		protected internal virtual async Task<string> GetSignOutIdAsync(string returnUrl)
		{
			// ReSharper disable InvertIf
			if(this.Facade.FeatureManager.IsEnabled(Feature.Saml))
			{
				var validatedSamlMessage = await this.Facade.SamlInteraction.GetRequestContext(returnUrl);

				// If it is a saml-force-authentication-request it is initiated from a service-provider and we create a sign-out-id to use.
				if(validatedSamlMessage?.Message is Saml2Request { ForceAuthentication: true })
					return await this.Facade.Interaction.CreateLogoutContextAsync();
			}
			// ReSharper restore InvertIf

			return null;
		}

		public virtual async Task<IActionResult> Index(string returnUrl)
		{
			if(!this.ClaimsSelectionFeaterEnabled)
				return this.NotFound();

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			if(!this.AuthenticateResult.Succeeded)
				return await this.RedirectToSignIn(returnUrl);

			if(this.ClaimsSelectionContext == null)
				return this.NotFound();

			var model = await this.CreateClaimsSelectionViewModelAsync(returnUrl);

			return this.View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IActionResult> Index(ClaimsSelectionForm form, string returnUrl)
		{
			if(!this.ClaimsSelectionFeaterEnabled)
				return this.NotFound();

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			if(!this.AuthenticateResult.Succeeded)
				return await this.RedirectToSignIn(returnUrl);

			if(this.ClaimsSelectionContext == null)
				return this.NotFound();

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var model = await this.CreateClaimsSelectionViewModelAsync(returnUrl);

			if(form.Cancel)
			{
				if(model.Form.RequiredSelectionsSelected)
					return await this.RedirectAsync(returnUrl);
			}

			foreach(var result in model.Results)
			{
				if(!result.Complete && result.Selector.SelectionRequired)
					model.Errors.Add(this.Localizer.GetString($"errors/{model.Form.AuthenticationScheme}/{await this.GetFullTypeNameWithUnderscoresInsteadOfDotsAsync(result.Selector)}/selection-is-not-complete"));
			}

			// ReSharper disable InvertIf
			if(!model.Errors.Any())
			{
				try
				{
					var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(returnUrl);
					var claimsPrincipal = await this.CreateClaimsPrincipalAsync(model.Results);

					if(this.User.IsAuthenticated())
						await this.Facade.Identity.SignOutAsync();
					else
						await this.HttpContext.SignOutAsync(this.IntermediateAuthenticationScheme);

					await this.HttpContext.SignInAsync(this.IntermediateAuthenticationScheme, claimsPrincipal, authenticationProperties);

					return this.Redirect(authenticationProperties.RedirectUri);
				}
				catch(Exception exception)
				{
					this.Logger.LogErrorIfEnabled(exception, "Could not set selected claims.");

					model.Errors.Add(this.Localizer.GetString("errors/unexpected-error"));
				}
			}
			// ReSharper restore InvertIf

			return this.View(model);
		}

		protected internal virtual async Task<IActionResult> RedirectAsync(string returnUrl)
		{
			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			if(authorizationRequest != null && authorizationRequest.IsNativeClient())
				return await this.Redirect(returnUrl, this.Facade.IdentityServer.CurrentValue.SignOut.SecondsBeforeRedirectAfterSignOut);

			return this.Redirect(returnUrl);
		}

		protected internal virtual async Task<IActionResult> RedirectToSignIn(string returnUrl)
		{
			var returnUrlParameterName = this.IdentityServer.UserInteraction.LoginReturnUrlParameter;
			var signInPath = this.IdentityServer.UserInteraction.LoginUrl;

			return await this.RedirectAsync(signInPath + QueryString.Create(returnUrlParameterName, returnUrl));
		}

		protected internal override string ResolveReturnUrl(string returnUrl)
		{
			return string.IsNullOrEmpty(returnUrl) || string.Equals(returnUrl, "~/", StringComparison.OrdinalIgnoreCase) ? "/Account" : returnUrl;
		}

		#endregion
	}
}