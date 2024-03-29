using System;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using HansKindberg.IdentityServer.Application.Models.Views.Consent;
using HansKindberg.IdentityServer.Models.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	[Authorize]
	public class ConsentController : ConsentControllerBase
	{
		#region Constructors

		public ConsentController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(string returnUrl)
		{
			return await this.CreateConsentViewModelAsync(await this.GetAuthorizationRequestAsync(returnUrl), null);
		}

		protected internal virtual async Task<AuthorizationRequest> GetAuthorizationRequestAsync(string returnUrl)
		{
			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			if(authorizationRequest == null)
				throw new InvalidOperationException($"Could not get a valid authorization-request from return-url \"{returnUrl}\".");

			return authorizationRequest;
		}

		public virtual async Task<IActionResult> Index(string returnUrl)
		{
			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			return this.View(await this.CreateConsentViewModelAsync(returnUrl));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public virtual async Task<IActionResult> Index(ConsentForm form, string returnUrl)
		{
			if(form == null)
				throw new ArgumentNullException(nameof(form));

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			var authorizationRequest = await this.GetAuthorizationRequestAsync(returnUrl);

			if(form.Accept)
				await this.ValidateConsentAsync(authorizationRequest, form);

			if(!this.ModelState.IsValid)
				return this.View(await this.CreateConsentViewModelAsync(authorizationRequest, form));

			var consentResponse = await (form.Accept ? this.AcceptConsentAsync(authorizationRequest, form) : this.RejectConsentAsync(authorizationRequest));

			await this.Facade.Interaction.GrantConsentAsync(authorizationRequest, consentResponse);

			return authorizationRequest.IsNativeClient() ? await this.Redirect(returnUrl, this.Facade.IdentityServer.CurrentValue.SignOut.SecondsBeforeRedirectAfterSignOut) : this.Redirect(returnUrl);
		}

		#endregion
	}
}