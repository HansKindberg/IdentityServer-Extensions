using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public class AuthenticateController : AuthenticateControllerBase
	{
		#region Constructors

		public AuthenticateController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		public virtual async Task<IActionResult> Callback()
		{
			this.Logger.LogDebugIfEnabled("Callback: starting...");

			var authenticateResult = await this.HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

			if(!authenticateResult.Succeeded)
			{
				this.Logger.LogErrorIfEnabled($"An authentication-attempt, using the scheme \"{IdentityServerConstants.ExternalCookieAuthenticationScheme}\", was made but failed. Available request-cookies: {string.Join(", ", this.Request.Cookies.Keys)}");

				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);
			}

			var returnUrl = this.ResolveAndValidateReturnUrl(authenticateResult.Properties.Items[AuthenticationKeys.ReturnUrl]);

			var authenticationScheme = authenticateResult.Properties.Items[AuthenticationKeys.Scheme];
			await this.ValidateAuthenticationSchemeForClientAsync(authenticationScheme, returnUrl);

			this.Logger.LogDebugIfEnabled($"Callback: authentication-sheme = \"{authenticationScheme}\", claims received = \"{string.Join(", ", authenticateResult.Principal.Claims.Select(claim => claim.Type))}\".");

			var decorators = (await this.Facade.DecorationLoader.GetCallbackDecoratorsAsync(authenticationScheme)).ToArray();

			if(!decorators.Any())
				throw new InvalidOperationException($"There are no callback-decorators for authentication-scheme \"{authenticationScheme}\".");

			var authenticationProperties = new AuthenticationProperties();
			var claims = new ClaimBuilderCollection();

			foreach(var decorator in decorators)
			{
				await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);

				this.Logger.LogDebugIfEnabled($"Callback: {decorator.GetType().FullName}.DecorateAsync claims = \"{string.Join(", ", claims.Select(claim => claim.Type))}\".");
			}

			await this.ConvertToJwtClaimsAsync(claims);

			this.Logger.LogDebugIfEnabled($"Callback: converted to jwt-claims = \"{string.Join(", ", claims.Select(claim => claim.Type))}\".");

			var user = await this.ResolveUserAsync(authenticationScheme, claims);

			await this.HttpContext.SignInAsync(user, authenticationProperties);

			await this.HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			await this.Facade.Events.RaiseAsync(new UserLoginSuccessEvent(user.IdentityProvider, user.ProviderUserId, user.SubjectId, user.DisplayName, true, authorizationRequest?.Client.ClientId));

			var claimsSelectionContext = await this.Facade.ClaimsSelectionContextAccessor.GetAsync(authenticationScheme, returnUrl);
			if(claimsSelectionContext != null)
				return this.Redirect(claimsSelectionContext.Url.ToString());

			if(authorizationRequest != null && authorizationRequest.IsNativeClient())
				return await this.Redirect(returnUrl, this.Facade.IdentityServer.CurrentValue.SignOut.SecondsBeforeRedirectAfterSignOut);

			return this.Redirect(returnUrl);
		}

		public virtual async Task<IActionResult> Certificate(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Certificate, returnUrl);

			var certificate = await this.HttpContext.Connection.GetClientCertificateAsync();

			if(certificate == null)
			{
				if(await this.Facade.MutualTlsService.IsMtlsDomainRequestAsync(this.Request))
					throw new InvalidOperationException("Authentication error.", new InvalidOperationException($"There is no client-certificate connected for url \"{this.Request.GetDisplayUrl()}\"."));

				var mtlsOrigin = await this.Facade.MutualTlsService.GetMtlsOriginAsync();
				var origin = this.Request.Origin();

				if(!string.Equals(mtlsOrigin, origin, StringComparison.OrdinalIgnoreCase))
					return this.Redirect($"{mtlsOrigin}{this.Request.Path}{this.Request.QueryString}");
			}

			var authenticateResult = await this.HttpContext.AuthenticateAsync(authenticationScheme);

			if(!authenticateResult.Succeeded)
				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);

			var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(authenticationScheme, returnUrl);
			var certificatePrincipal = authenticateResult.Principal;
			var decorators = (await this.Facade.DecorationLoader.GetAuthenticationDecoratorsAsync(authenticationScheme)).ToArray();

			if(decorators.Any())
			{
				var claims = new ClaimBuilderCollection();

				foreach(var decorator in decorators)
				{
					await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);
				}

				certificatePrincipal = this.CreateClaimsPrincipal(authenticationScheme, claims);
			}

			await this.HttpContext.SignInAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme, certificatePrincipal, authenticationProperties);

			return this.Redirect(authenticationProperties.RedirectUri);
		}

		protected internal virtual async Task ConvertToJwtClaimsAsync(IClaimBuilderCollection claims)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			await Task.CompletedTask;

			foreach(var mapping in this.Facade.IdentityServer.CurrentValue.ClaimTypeMap)
			{
				foreach(var claim in claims)
				{
					if(string.Equals(mapping.Key.UrlDecodeColon(), claim.Type, StringComparison.OrdinalIgnoreCase))
						claim.Type = mapping.Value;
				}
			}
		}

		protected internal virtual async Task<AuthenticationProperties> CreateAuthenticationPropertiesAsync(string authenticationScheme, string returnUrl)
		{
			var authenticationProperties = new AuthenticationProperties
			{
				RedirectUri = this.Url.Action(nameof(this.Callback))
			};

			if(await this.Facade.MutualTlsService.IsMtlsDomainRequestAsync(this.Request))
				authenticationProperties.RedirectUri = await this.Facade.MutualTlsService.GetIssuerOriginAsync() + authenticationProperties.RedirectUri;

			authenticationProperties.SetString(AuthenticationKeys.ReturnUrl, returnUrl);
			authenticationProperties.SetString(AuthenticationKeys.Scheme, authenticationScheme);

			foreach(var decorator in await this.Facade.DecorationLoader.GetAuthenticationPropertiesDecoratorsAsync(authenticationScheme))
			{
				await decorator.DecorateAsync(authenticationScheme, authenticationProperties, returnUrl);
			}

			return authenticationProperties;
		}

		protected internal virtual ClaimsPrincipal CreateClaimsPrincipal(string authenticationScheme, IClaimBuilderCollection claims)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			return new ClaimsPrincipal(new ClaimsIdentity(claims.Build(), authenticationScheme, claims.FindFirstNameClaim()?.Type, null));
		}

		public virtual async Task<IActionResult> Negotiate(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Negotiate, returnUrl);

			// Check if negotiate-authentication has already been requested and succeeded.
			var authenticateResult = await this.HttpContext.AuthenticateAsync(authenticationScheme);

			// ReSharper disable InvertIf
			if(authenticateResult?.Principal is WindowsPrincipal)
			{
				var decorators = (await this.Facade.DecorationLoader.GetAuthenticationDecoratorsAsync(authenticationScheme)).ToArray();

				if(!decorators.Any())
					throw new InvalidOperationException($"There are no authentication-decorators for authentication-scheme \"{authenticationScheme}\".");

				var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(authenticationScheme, returnUrl);
				var claims = new ClaimBuilderCollection();

				foreach(var decorator in decorators)
				{
					await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);
				}

				await this.HttpContext.SignInAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme, this.CreateClaimsPrincipal(authenticationScheme, claims), authenticationProperties);

				return this.Redirect(authenticationProperties.RedirectUri);
			}
			// ReSharper restore InvertIf

			// Trigger negotiate-authentication. Since negotiate-authentication don't support the redirect uri, this URL is re-triggered when we call challenge.
			return this.Challenge(authenticationScheme);
		}

		public virtual async Task<IActionResult> Remote(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Remote, returnUrl);

			var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(authenticationScheme, returnUrl);

			return this.Challenge(authenticationProperties, authenticationScheme);
		}

		protected internal virtual async Task<string> ResolveAndValidateAsync(string authenticationScheme, AuthenticationSchemeKind expectedAuthenticationSchemeKind, string returnUrl)
		{
			await this.ValidateAuthenticationSchemeAsync(expectedAuthenticationSchemeKind, authenticationScheme);

			returnUrl = this.ResolveAndValidateReturnUrl(returnUrl);

			await this.ValidateAuthenticationSchemeForClientAsync(authenticationScheme, returnUrl);

			return returnUrl;
		}

		protected internal virtual async Task ValidateAuthenticationSchemeAsync(AuthenticationSchemeKind expectedKind, string name)
		{
			if(name == null)
				throw new ArgumentNullException(nameof(name));

			var authenticationScheme = await this.Facade.AuthenticationSchemeRetriever.GetAsync(name);

			if(authenticationScheme == null)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" does not exist.");

			if(!authenticationScheme.Enabled)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not enabled.");

			if(!authenticationScheme.Interactive)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not interactive.");

			if(authenticationScheme.Kind != expectedKind)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not of kind \"{expectedKind}\".");

			if(authenticationScheme.Kind != AuthenticationSchemeKind.Certificate && await this.Facade.MutualTlsService.IsMtlsDomainRequestAsync(this.Request))
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not allowed on the mTLS-domain. Only certificate-authentication is allowed on the mTLS-domain.");
		}

		protected internal virtual async Task ValidateAuthenticationSchemeForClientAsync(string authenticationScheme, string returnUrl)
		{
			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			var clientId = authorizationRequest?.Client?.ClientId;

			if(clientId == null)
				return;

			var client = await this.Facade.ClientStore.FindEnabledClientByIdAsync(clientId);

			if(client == null)
				return;

			var authenticationSchemeRestrictions = (client.IdentityProviderRestrictions ?? Enumerable.Empty<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);

			if(!authenticationSchemeRestrictions.Any())
				return;

			if(!authenticationSchemeRestrictions.Contains(authenticationScheme))
				throw new InvalidOperationException($"The authentication-scheme \"{authenticationScheme}\" is not valid for client \"{clientId}\".");
		}

		#endregion
	}
}