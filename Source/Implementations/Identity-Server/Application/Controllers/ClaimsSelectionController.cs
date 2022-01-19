using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement.Mvc;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using Rsk.Saml.Models;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	[Authorize]
	[FeatureGate(Feature.ClaimsSelection)]
	public class ClaimsSelectionController : AuthenticateControllerBase
	{
		#region Fields

		private const string _authenticationType = "Duende.IdentityServer"; // Duende.IdentityServer.Constants.IdentityServerAuthenticationType
		private Lazy<IClaimsSelectionContext> _claimsSelectionContext;

		#endregion

		#region Constructors

		public ClaimsSelectionController(IFacade facade) : base(facade) { }

		#endregion

		#region Properties

		protected internal virtual string AuthenticationType => _authenticationType;

		protected internal virtual IClaimsSelectionContext ClaimsSelectionContext
		{
			get
			{
				this._claimsSelectionContext ??= new Lazy<IClaimsSelectionContext>(() => this.Facade.ClaimsSelectionContextAccessor.ClaimsSelectionContext);

				return this._claimsSelectionContext.Value;
			}
		}

		#endregion

		#region Methods

		protected internal virtual async Task<ClaimsSelectionConfirmationViewModel> CreateClaimsSelectionConfirmationViewModelAsync(string returnUrl, string signOutId)
		{
			var model = await this.CreateSingleSignOutViewModelAsync<ClaimsSelectionConfirmationViewModel>(signOutId);

			model.AuthenticationScheme = this.ClaimsSelectionContext.AuthenticationScheme;
			model.RedirectUrl = returnUrl;

			return model;
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
					AuthenticationScheme = this.ClaimsSelectionContext.AuthenticationScheme
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

				var result = await claimsSelector.SelectAsync(selections);

				model.Results.Add(result);

				foreach(var (key, value) in result.Selectables)
				{
					model.Form.SelectableClaims.Add($"{inputPrefix}{key}", value);
				}
			}

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
			this.ValidateContext();

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			var model = await this.CreateClaimsSelectionViewModelAsync(returnUrl);

			return this.View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IActionResult> Index(ClaimsSelectionForm form, string returnUrl)
		{
			this.ValidateContext();

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			if(form.Cancel)
				return await this.RedirectAsync(returnUrl);

			var model = await this.CreateClaimsSelectionViewModelAsync(returnUrl);

			foreach(var result in model.Results)
			{
				if(!result.Complete)
					model.Errors.Add(this.Localizer.GetString($"errors/{model.Form.AuthenticationScheme}/{await this.GetFullTypeNameWithUnderscoresInsteadOfDotsAsync(result.Selector)}/selection-is-not-complete"));
			}

			// ReSharper disable InvertIf
			if(!model.Errors.Any())
			{
				var signOutId = await this.GetSignOutIdAsync(returnUrl);

				var confirmationModel = await this.CreateClaimsSelectionConfirmationViewModelAsync(returnUrl, signOutId);

				try
				{
					await this.UpdateSignInAsync(model.Results);

					return this.View("Confirmation", confirmationModel);
				}
				catch(Exception exception)
				{
					this.Logger.LogErrorIfEnabled(exception, "Could not refresh sign-in.");

					model.Errors.Add(this.Localizer.GetString("errors/update-sign-in-error"));
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

		protected internal virtual async Task UpdateSignInAsync(IEnumerable<IClaimsSelectionResult> claimsSelectionResults)
		{
			if(claimsSelectionResults == null)
				throw new ArgumentNullException(nameof(claimsSelectionResults));

			claimsSelectionResults = claimsSelectionResults.ToArray();

			var claimsIdentity = this.User.Identities.First(claimsIdentity => string.Equals(this.AuthenticationType, claimsIdentity.AuthenticationType, StringComparison.OrdinalIgnoreCase));

			var claims = new ClaimBuilderCollection();
			claims.AddRange(claimsIdentity.Claims.Select(claim => new ClaimBuilder(claim)));

			foreach(var result in claimsSelectionResults)
			{
				var selectedClaims = await result.Selector.GetClaimsAsync(result);

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

			var user = await this.ResolveUserAsync(this.ClaimsSelectionContext.AuthenticationScheme, claims);

			await this.HttpContext.SignInAsync(user, new AuthenticationProperties());
		}

		protected internal virtual void ValidateContext()
		{
			if(!this.Facade.FeatureManager.IsEnabled(Feature.ClaimsSelection))
				throw new InvalidOperationException($"The feature \"{Feature.ClaimsSelection}\" is not enabled.");

			if(!this.User.IsAuthenticated())
				throw new UnauthorizedAccessException("The http-context-user is not authenticated.");

			if(this.ClaimsSelectionContext == null)
				throw new InvalidOperationException("The current claims-selection-context is null.");
		}

		#endregion
	}
}