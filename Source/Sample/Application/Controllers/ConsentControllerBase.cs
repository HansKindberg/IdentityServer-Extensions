using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Models.Views.Consent;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using HansKindberg.IdentityServer;
using RegionOrebroLan.Collections.Generic.Extensions;

namespace Application.Controllers
{
	public abstract class ConsentControllerBase : SiteController
	{
		#region Constructors

		protected ConsentControllerBase(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		protected internal virtual async Task<ConsentResponse> AcceptConsentAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			var consentResponse = new ConsentResponse
			{
				Description = form.Description,
				RememberConsent = form.Persistent,
				ScopesValuesConsented = form.ConsentedIdentityResources.Concat(form.ConsentedApiScopes).ToArray()
			};

			await this.Facade.Events.RaiseAsync(new ConsentGrantedEvent(this.User.GetSubjectId(), authorizationRequest.Client.ClientId, authorizationRequest.ValidatedResources.RawScopeValues, consentResponse.ScopesValuesConsented, consentResponse.RememberConsent));

			return consentResponse;
		}

		protected internal virtual async Task<ConsentForm> CreateConsentFormAsync(AuthorizationRequest authorizationRequest, ConsentForm postedForm, string returnUrl)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			var form = new ConsentForm
			{
				ReturnUrl = returnUrl
			};

			form.IdentityResources.Add(authorizationRequest.ValidatedResources.Resources.IdentityResources.Select(this.CreateScopeViewModel));
			form.ApiScopes.Add(authorizationRequest.ValidatedResources.Resources.ApiScopes.Select(this.CreateScopeViewModel));

			if(this.Facade.IdentityServer.CurrentValue.Consent.OfflineAccessEnabled && authorizationRequest.ValidatedResources.Resources.OfflineAccess)
			{
				form.ApiScopes.Add(new ScopeViewModel
				{
					Checked = true,
					Emphasize = true,
					Name = IdentityServerConstants.StandardScopes.OfflineAccess
				});
			}

			// ReSharper disable InvertIf
			if(postedForm != null)
			{
				foreach(var (key, value) in postedForm.Dictionary)
				{
					form.Dictionary.Add(key, value);
				}

				foreach(var scope in form.IdentityResources)
				{
					scope.Checked = postedForm.ConsentedIdentityResources.Contains(scope.Name, StringComparer.OrdinalIgnoreCase);
				}

				foreach(var scope in form.ApiScopes)
				{
					scope.Checked = postedForm.ConsentedApiScopes.Contains(scope.Name, StringComparer.OrdinalIgnoreCase);
				}
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(form);
		}

		protected internal virtual async Task<ConsentViewModel> CreateConsentViewModelAsync(AuthorizationRequest authorizationRequest, ConsentForm postedForm, string returnUrl)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			var model = new ConsentViewModel
			{
				Client = authorizationRequest.Client,
				Form = await this.CreateConsentFormAsync(authorizationRequest, postedForm, returnUrl),
				PersistenceEnabled = authorizationRequest.Client.AllowRememberConsent
			};

			return await Task.FromResult(model);
		}

		protected internal virtual ScopeViewModel CreateScopeViewModel(IdentityResource identityResource)
		{
			if(identityResource == null)
				throw new ArgumentNullException(nameof(identityResource));

			var model = this.CreateScopeViewModelInternal(identityResource);

			model.Emphasize = identityResource.Emphasize;
			model.Required = identityResource.Required;

			return model;
		}

		protected internal virtual ScopeViewModel CreateScopeViewModel(ApiScope apiScope)
		{
			if(apiScope == null)
				throw new ArgumentNullException(nameof(apiScope));

			var model = this.CreateScopeViewModelInternal(apiScope);

			model.Emphasize = apiScope.Emphasize;
			model.Required = apiScope.Required;

			return model;
		}

		protected internal virtual ScopeViewModel CreateScopeViewModelInternal(Resource resource)
		{
			if(resource == null)
				throw new ArgumentNullException(nameof(resource));

			return new ScopeViewModel
			{
				Checked = true,
				Description = resource.Description,
				DisplayName = resource.DisplayName ?? resource.Name,
				Name = resource.Name,
			};
		}

		protected internal virtual async Task<ConsentResponse> RejectConsentAsync(AuthorizationRequest authorizationRequest)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			await this.Facade.Events.RaiseAsync(new ConsentDeniedEvent(this.User.GetSubjectId(), authorizationRequest.Client.ClientId, authorizationRequest.ValidatedResources.RawScopeValues));

			return new ConsentResponse { Error = AuthorizationError.AccessDenied };
		}

		protected internal virtual async Task ValidateConsentAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			await this.ValidateRequiredIdentityResourcesAsync(authorizationRequest, form);
			await this.ValidateOfflineAccessAsync(authorizationRequest, form);
			await this.ValidateInvalidIdentityResourcesAsync(authorizationRequest, form);
			await this.ValidateInvalidApiScopesAsync(authorizationRequest, form);
			await this.ValidateRememberConsentAsync(authorizationRequest, form);
		}

		protected internal virtual async Task ValidateInvalidApiScopesAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			await Task.CompletedTask;

			const string nullKey = "null-api-scope";

			foreach(var key in form.ConsentedApiScopes.Where(key => !string.Equals(key, IdentityServerConstants.StandardScopes.OfflineAccess, StringComparison.OrdinalIgnoreCase)))
			{
				if(!authorizationRequest.ValidatedResources.Resources.ApiScopes.Any(apiScope => string.Equals(key, apiScope.Name, StringComparison.OrdinalIgnoreCase)))
					this.ModelState.AddModelError(key ?? nullKey, this.GetLocalizedValue("errors/invalid-api-scope", key ?? nullKey));
			}
		}

		protected internal virtual async Task ValidateInvalidIdentityResourcesAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			await Task.CompletedTask;

			const string nullKey = "null-identity-resource";

			foreach(var key in form.ConsentedIdentityResources)
			{
				if(!authorizationRequest.ValidatedResources.Resources.IdentityResources.Any(identityResource => string.Equals(key, identityResource.Name, StringComparison.OrdinalIgnoreCase)))
					this.ModelState.AddModelError(key ?? nullKey, this.GetLocalizedValue("errors/invalid-identity-resource", key ?? nullKey));
			}
		}

		protected internal virtual async Task ValidateOfflineAccessAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			await Task.CompletedTask;

			const string offlineAccessScopeName = IdentityServerConstants.StandardScopes.OfflineAccess;

			if(!form.ConsentedApiScopes.Contains(offlineAccessScopeName, StringComparer.OrdinalIgnoreCase))
				return;

			if(!this.Facade.IdentityServer.CurrentValue.Consent.OfflineAccessEnabled)
				this.ModelState.AddModelError(offlineAccessScopeName, this.GetLocalizedValue("errors/not-enabled", offlineAccessScopeName));

			if(!authorizationRequest.ValidatedResources.Resources.OfflineAccess)
				this.ModelState.AddModelError(offlineAccessScopeName, this.GetLocalizedValue("errors/not-enabled-for-client", offlineAccessScopeName));
		}

		protected internal virtual async Task ValidateRememberConsentAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			await Task.CompletedTask;

			if(form.Persistent && !authorizationRequest.Client.AllowRememberConsent)
				this.ModelState.AddModelError(nameof(form.Persistent), this.GetLocalizedValue("errors/not-allowed", nameof(form.Persistent)));
		}

		protected internal virtual async Task ValidateRequiredIdentityResourcesAsync(AuthorizationRequest authorizationRequest, ConsentForm form)
		{
			if(authorizationRequest == null)
				throw new ArgumentNullException(nameof(authorizationRequest));

			if(form == null)
				throw new ArgumentNullException(nameof(form));

			await Task.CompletedTask;

			foreach(var requiredIdentityResource in authorizationRequest.ValidatedResources.Resources.IdentityResources.Where(identityResource => identityResource.Required))
			{
				if(!form.ConsentedIdentityResources.Contains(requiredIdentityResource.Name, StringComparer.OrdinalIgnoreCase))
					this.ModelState.AddModelError(requiredIdentityResource.Name, this.GetLocalizedValue("errors/required", requiredIdentityResource.Name));
			}
		}

		#endregion
	}
}