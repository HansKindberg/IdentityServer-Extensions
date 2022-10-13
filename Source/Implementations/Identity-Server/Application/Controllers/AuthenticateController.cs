using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Configuration;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public class AuthenticateController : AuthenticateControllerBase
	{
		#region Constructors

		public AuthenticateController(IFacade facade) : base(facade)
		{
			this.Authentication = ((facade ?? throw new ArgumentNullException(nameof(facade))).Authentication ?? throw new ArgumentException("The authentication-property can not be null.", nameof(facade))).CurrentValue;
			this.IdentityServer = (facade.IdentityServer ?? throw new ArgumentException("The identity-server-property can not be null.", nameof(facade))).CurrentValue;
		}

		#endregion

		#region Properties

		protected internal virtual ExtendedAuthenticationOptions Authentication { get; }
		protected internal virtual ExtendedIdentityServerOptions IdentityServer { get; }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Callback()
		{
			return await this.CallbackInternal(IdentityServerConstants.ExternalCookieAuthenticationScheme);
		}

		public virtual async Task<IActionResult> CallbackCertificate()
		{
			return await this.CallbackInternal(this.IdentityServer.IntermediateCookieAuthenticationHandlers.Certificate.Name);
		}

		/// <summary>
		/// So we can use the callback action with different intermediate cookie-authentication-schemes. The usual one is "idsrv.external".
		/// </summary>
		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		protected internal virtual async Task<IActionResult> CallbackInternal(string intermediateCookieAuthenticationScheme)
		{
			if(intermediateCookieAuthenticationScheme == null)
				throw new ArgumentNullException(nameof(intermediateCookieAuthenticationScheme));

			this.Logger.LogDebugIfEnabled("Callback: starting...");

			var authenticateResult = await this.HttpContext.AuthenticateAsync(intermediateCookieAuthenticationScheme);

			if(!authenticateResult.Succeeded)
			{
				this.Logger.LogErrorIfEnabled($"An authentication-attempt, using the scheme \"{intermediateCookieAuthenticationScheme}\", was made but failed. Available request-cookies: {string.Join(", ", this.Request.Cookies.Keys)}");

				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);
			}

			var returnUrl = this.ResolveAndValidateReturnUrl(authenticateResult.Properties?.Items[AuthenticationKeys.ReturnUrl]);

			var authenticationScheme = authenticateResult.Properties?.Items[AuthenticationKeys.Scheme];
			await this.ValidateAuthenticationSchemeForClientAsync(authenticationScheme, returnUrl);

			this.Logger.LogDebugIfEnabled($"Callback: authentication-sheme = \"{authenticationScheme}\", claims received = \"{string.Join(", ", authenticateResult.Principal.Claims.Select(claim => claim.Type))}\".");

			var decorators = (await this.Facade.DecorationLoader.GetCallbackDecoratorsAsync(authenticationScheme)).ToArray();

			var authenticationProperties = new AuthenticationProperties();
			var claims = new ClaimBuilderCollection();

			if(decorators.Any())
			{
				foreach(var decorator in decorators)
				{
					await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);

					this.Logger.LogDebugIfEnabled($"Callback: {decorator.GetType().FullName}.DecorateAsync claims = \"{string.Join(", ", claims.Select(claim => claim.Type))}\".");
				}
			}
			else
			{
				this.Logger.LogDebugIfEnabled($"There are no callback-decorators for authentication-scheme \"{authenticationScheme}\".");
			}

			await this.ResolveRequiredClaims(authenticateResult, authenticationScheme, claims);

			await this.ConvertToJwtClaimsAsync(claims);

			this.Logger.LogDebugIfEnabled($"Callback: converted to jwt-claims = \"{string.Join(", ", claims.Select(claim => claim.Type))}\".");

			var user = await this.ResolveUserAsync(authenticationScheme, claims);

			await this.ResolveAuthenticationLocally(authenticateResult, authenticationProperties, user);

			await this.HttpContext.SignInAsync(user, authenticationProperties);

			await this.HttpContext.SignOutAsync(intermediateCookieAuthenticationScheme);

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

			var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(authenticationScheme, this.Url.Action(nameof(this.CallbackCertificate)), returnUrl);
			var certificatePrincipal = authenticateResult.Principal;
			var decorators = (await this.Facade.DecorationLoader.GetAuthenticationDecoratorsAsync(authenticationScheme)).ToArray();

			if(decorators.Any())
			{
				var claims = new ClaimBuilderCollection();

				foreach(var decorator in decorators)
				{
					await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);
				}

				certificatePrincipal = await this.CreateClaimsPrincipalAsync(authenticationScheme, claims);
			}

			await this.HttpContext.SignInAsync(this.IdentityServer.IntermediateCookieAuthenticationHandlers.Certificate.Name, certificatePrincipal, authenticationProperties);

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
			return await this.CreateAuthenticationPropertiesAsync(authenticationScheme, this.Url.Action(nameof(this.Callback)), returnUrl);
		}

		protected internal virtual async Task<AuthenticationProperties> CreateAuthenticationPropertiesAsync(string authenticationScheme, string redirectUri, string returnUrl)
		{
			var authenticationProperties = new AuthenticationProperties
			{
				RedirectUri = redirectUri
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

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string authenticationScheme, IClaimBuilderCollection claims)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			return await Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(claims.Build(), authenticationScheme, claims.FindFirstNameClaim()?.Type, claims.FindFirst(ClaimTypes.Role, JwtClaimTypes.Role)?.Type)));
		}

		public virtual async Task<IActionResult> Negotiate(string authenticationScheme, string returnUrl)
		{
			returnUrl = await this.ResolveAndValidateAsync(authenticationScheme, AuthenticationSchemeKind.Negotiate, returnUrl);

			// Check if negotiate-authentication has already been requested and succeeded.
			var authenticateResult = await this.HttpContext.AuthenticateAsync(authenticationScheme);

			// ReSharper disable InvertIf
			if(authenticateResult.Succeeded)
			{
				if(authenticateResult.Principal == null)
					throw new InvalidOperationException("Succeeded authenticate-result but the principal is null.");

				var authenticationProperties = await this.CreateAuthenticationPropertiesAsync(authenticationScheme, returnUrl);
				var claims = new ClaimBuilderCollection();
				var decorators = (await this.Facade.DecorationLoader.GetAuthenticationDecoratorsAsync(authenticationScheme)).ToArray();

				if(decorators.Any())
				{
					foreach(var decorator in decorators)
					{
						await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);
					}
				}
				else
				{
					var nameClaim = authenticateResult.Principal.Claims.FindFirstNameClaim();

					if(nameClaim != null)
						claims.Add(new ClaimBuilder(nameClaim));

					var uniqueIdentifierClaim = authenticateResult.Principal.Claims.FindFirst(this.Authentication.Negotiate.UniqueIdentifierClaimType);

					if(uniqueIdentifierClaim == null)
						throw new InvalidOperationException($"Could not find an unique identifier claim. Claim-type uses as unique identifier claim-type is {this.Authentication.Negotiate.UniqueIdentifierClaimType.ToStringRepresentation()}.");

					claims.Add(new ClaimBuilder { Type = ClaimTypes.NameIdentifier, Value = uniqueIdentifierClaim.Value });

					if(this.Authentication.Negotiate.IncludeSecurityIdentifierClaim)
					{
						var securityIdentifierClaim = authenticateResult.Principal.Claims.FindFirst(ClaimTypes.PrimarySid);

						if(securityIdentifierClaim != null)
							claims.Add(new ClaimBuilder(securityIdentifierClaim));
					}

					if(this.Authentication.Negotiate.IncludeNameClaimAsWindowsAccountNameClaim && nameClaim != null)
						claims.Add(new ClaimBuilder { Type = ClaimTypes.WindowsAccountName, Value = nameClaim.Value });

					if(this.Authentication.Negotiate.Roles.Include)
					{
						/*
							If there are many roles and we save the principal in the authentication-cookie we may get an error:
							Bad Request - Request Too Long: HTTP Error 400. The size of the request headers is too long.

							We can handle that by implementing a CookieAuthenticationOptions.SessionStore (ITicketStore).

							Still, it is not recommended to save roles as claims. Identity vs Permissions: https://leastprivilege.com/2016/12/16/identity-vs-permissions/
						*/
						var roles = new List<string>();

						foreach(var roleClaim in authenticateResult.Principal.Claims.Find(this.Authentication.Negotiate.Roles.ClaimType))
						{
							var role = roleClaim.Value;

							if(this.Authentication.Negotiate.Roles.Translate && OperatingSystem.IsWindows())
							{
								var securityIdentifier = new SecurityIdentifier(role);
								role = securityIdentifier.Translate(typeof(NTAccount)).Value;
							}

							roles.Add(role);
						}

						roles.Sort();

						// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
						foreach(var role in roles)
						{
							claims.Add(new ClaimBuilder { Type = ClaimTypes.Role, Value = role });
						}
						// ReSharper restore ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
					}
				}

				await this.HttpContext.SignInAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme, await this.CreateClaimsPrincipalAsync(authenticationScheme, claims), authenticationProperties);

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

		protected internal virtual async Task ResolveAuthenticationLocally(AuthenticateResult authenticateResult, AuthenticationProperties authenticationProperties, ExtendedIdentityServerUser user)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			if(authenticationProperties == null)
				throw new ArgumentNullException(nameof(authenticationProperties));

			if(user == null)
				throw new ArgumentNullException(nameof(user));

			await Task.CompletedTask;

			// If the external provider issued an id_token, we'll keep it for sign-out.
			var identityToken = authenticateResult.Properties?.GetTokenValue(OidcConstants.TokenTypes.IdentityToken);

			if(identityToken != null)
			{
				authenticationProperties.StoreTokens(new[] { new AuthenticationToken { Name = OidcConstants.TokenTypes.IdentityToken, Value = identityToken } });
				this.Logger.LogDebugIfEnabled("An identity-token was added.");
			}
			else
			{
				this.Logger.LogDebugIfEnabled("No identity-token to add.");
			}

			// If the external provider sent a session id claim, we'll copy it over for sign-out.
			var sessionIdClaim = authenticateResult.Principal?.Claims.FirstOrDefault(claim => string.Equals(JwtClaimTypes.SessionId, claim.Type, StringComparison.OrdinalIgnoreCase));

			if(sessionIdClaim != null)
			{
				user.AdditionalClaims.Add(new Claim(JwtClaimTypes.SessionId, sessionIdClaim.Value));
				this.Logger.LogDebugIfEnabled("A session-id-claim was added.");
			}
			else
			{
				this.Logger.LogDebugIfEnabled("No session-id-claim to add.");
			}
		}

		protected internal virtual async Task ResolveRequiredClaims(AuthenticateResult authenticateResult, string authenticationScheme, IClaimBuilderCollection claims)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			await Task.CompletedTask;

			var uniqueIdentifierClaim = claims.FindFirstUniqueIdentifierClaim();

			if(uniqueIdentifierClaim == null)
			{
				var principalUniqueIdentifierClaim = authenticateResult.Principal?.Claims.FindFirstUniqueIdentifierClaim();

				if(principalUniqueIdentifierClaim != null)
				{
					uniqueIdentifierClaim = new ClaimBuilder(principalUniqueIdentifierClaim);
					claims.Add(uniqueIdentifierClaim);
				}
			}

			if(uniqueIdentifierClaim == null)
				throw new InvalidOperationException($"There is no unique-identifier-claim for authentication-scheme \"{authenticationScheme}\".");

			var nameClaim = claims.FindFirstNameClaim();

			if(nameClaim == null)
			{
				var principalNameClaim = authenticateResult.Principal?.Claims.FindFirstNameClaim();

				if(principalNameClaim != null)
				{
					nameClaim = new ClaimBuilder(principalNameClaim);
					claims.Add(nameClaim);
				}
			}
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