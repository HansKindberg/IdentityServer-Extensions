using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Web.Authentication;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace Application.Controllers
{
	public class AuthenticateController : SiteController
	{
		#region Constructors

		public AuthenticateController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Callback()
		{
			var authenticateResult = await this.HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

			if(!authenticateResult.Succeeded)
				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);

			var returnUrl = this.ResolveAndValidateReturnUrl(authenticateResult.Properties.Items[AuthenticationKeys.ReturnUrl]);

			var authenticationScheme = authenticateResult.Properties.Items[AuthenticationKeys.Scheme];
			await this.ValidateAuthenticationSchemeForClientAsync(authenticationScheme, returnUrl);

			var decorators = (await this.Facade.DecorationLoader.GetCallbackDecoratorsAsync(authenticationScheme)).ToArray();

			if(!decorators.Any())
				throw new InvalidOperationException($"There are no callback-decorators for authentication-scheme \"{authenticationScheme}\".");

			var authenticationProperties = new AuthenticationProperties();
			var claims = new ClaimBuilderCollection();

			foreach(var decorator in decorators)
			{
				await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);
			}

			await this.ConvertToJwtClaimsAsync(claims);

			var user = await this.ResolveUserAsync(authenticationScheme, claims);

			await this.HttpContext.SignInAsync(user, authenticationProperties);

			await this.HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			await this.Facade.Events.RaiseAsync(new UserLoginSuccessEvent(user.IdentityProvider, user.ProviderUserId, user.SubjectId, user.DisplayName, true, authorizationRequest?.Client.ClientId));

			if(authorizationRequest != null && authorizationRequest.IsNativeClient())
				return await this.Redirect(returnUrl, this.Facade.IdentityServer.Value.Redirection.SecondsBeforeRedirect);

			return this.Redirect(returnUrl);
		}

		public virtual async Task<IActionResult> Certificate(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Certificate, returnUrl);

			var certificate = await this.HttpContext.Connection.GetClientCertificateAsync();

			if(certificate == null)
			{
				var request = this.HttpContext.Request;

				var certificateAuthenticationHost = await this.GetCertificateAuthenticationHostAsync();
				var certificateAuthenticationUrl = $"{request.Scheme}://{certificateAuthenticationHost}{request.Path}{request.QueryString}";
				var host = request.Host.Value;

				if(!string.Equals(certificateAuthenticationHost, host, StringComparison.OrdinalIgnoreCase))
					return this.Redirect(certificateAuthenticationUrl);

				throw new InvalidOperationException("Authentication error.", new InvalidOperationException($"There is no client-certificate connected for url \"{certificateAuthenticationUrl}\"."));
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

			foreach(var mapping in this.Facade.IdentityServer.Value.ClaimTypeMap)
			{
				foreach(var claim in claims)
				{
					if(string.Equals(mapping.Key, claim.Type, StringComparison.OrdinalIgnoreCase))
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

		protected internal virtual async Task<string> GetCertificateAuthenticationHostAsync()
		{
			var domainNameForCertificateAuthentication = this.Facade.IdentityServer.Value.InteractiveMutualTlsDomain;

			if(string.IsNullOrEmpty(domainNameForCertificateAuthentication))
				return this.HttpContext.Request.Host.Value;

			if(domainNameForCertificateAuthentication.Contains('.', StringComparison.OrdinalIgnoreCase))
				return domainNameForCertificateAuthentication;

			return await Task.FromResult($"{domainNameForCertificateAuthentication}.{this.HttpContext.Request.Host.Value}");
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

		protected internal virtual async Task<ExtendedIdentityServerUser> ResolveUserAsync(string authenticationScheme, IClaimBuilderCollection claims)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			claims = claims.Clone();

			var uniqueIdentifierClaim = claims.FindFirstUniqueIdentifierClaim();

			if(uniqueIdentifierClaim == null)
				throw new InvalidOperationException($"There is no unique-identifier-claim for authentication-scheme \"{authenticationScheme}\".");

			var uniqueIdentifier = uniqueIdentifierClaim.Value;
			claims.Remove(uniqueIdentifierClaim);

			var identityProviderClaim = claims.FindFirstIdentityProviderClaim();
			var identityProvider = identityProviderClaim?.Value ?? authenticationScheme;

			if(identityProviderClaim != null)
				claims.Remove(identityProviderClaim);

			var user = await this.Facade.Identity.ResolveUserAsync(claims, identityProvider, uniqueIdentifier);

			var nameClaim = claims.FindFirstNameClaim();
			var name = nameClaim?.Value;

			if(nameClaim != null)
				claims.Remove(nameClaim);

			return new ExtendedIdentityServerUser(user.Id)
			{
				AdditionalClaims = claims.Build(),
				DisplayName = name,
				IdentityProvider = identityProvider,
				ProviderUserId = uniqueIdentifier
			};
		}

		protected internal virtual async Task ValidateAuthenticationSchemeAsync(AuthenticationSchemeKind expectedKind, string name)
		{
			if(name == null)
				throw new ArgumentNullException(nameof(name));

			var authenticationScheme = await this.Facade.AuthenticationSchemeLoader.GetAsync(name);

			if(authenticationScheme == null)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" does not exist.");

			if(!authenticationScheme.Enabled)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not enabled.");

			if(!authenticationScheme.Interactive)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not interactive.");

			if(authenticationScheme.Kind != expectedKind)
				throw new InvalidOperationException($"The authentication-scheme \"{name}\" is not of kind \"{expectedKind}\".");
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

		public virtual async Task<IActionResult> Windows(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Windows, returnUrl);

			// Check if windows-authentication has already been requested and succeeded.
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

			// Trigger windows-authentication. Since windows-authentication don't support the redirect uri, this URL is re-triggered when we call challenge.
			return this.Challenge(authenticationScheme);
		}

		#endregion
	}
}