using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Models.Extensions;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.IdentityServer.Web.Authentication.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public abstract class AuthenticateControllerBase : SiteController
	{
		#region Constructors

		protected AuthenticateControllerBase(IFacade facade) : base(facade)
		{
			this.IdentityServer = (facade.IdentityServer ?? throw new ArgumentException("The identity-server-property can not be null.", nameof(facade))).CurrentValue;
		}

		#endregion

		#region Properties

		protected internal virtual ExtendedIdentityServerOptions IdentityServer { get; }

		#endregion

		#region Methods

		/// <summary>
		/// So we can use the callback action with different intermediate cookie-authentication-schemes. The usual one is "idsrv.external".
		/// </summary>
		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		protected internal virtual async Task<IActionResult> CallbackInternal(string intermediateCookieAuthenticationScheme)
		{
			if(intermediateCookieAuthenticationScheme == null)
				throw new ArgumentNullException(nameof(intermediateCookieAuthenticationScheme));

			this.Logger.LogDebugIfEnabled($"Callback internal for authentication-scheme {intermediateCookieAuthenticationScheme.ToStringRepresentation()} starting...");

			var authenticateResult = await this.HttpContext.AuthenticateAsync(intermediateCookieAuthenticationScheme);

			return await this.CallbackInternal(authenticateResult, intermediateCookieAuthenticationScheme);
		}

		/// <summary>
		/// So we can use the callback action with different intermediate cookie-authentication-schemes when we already have the authenticate-result.
		/// </summary>
		[SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling")]
		protected internal virtual async Task<IActionResult> CallbackInternal(AuthenticateResult authenticateResult, string intermediateCookieAuthenticationScheme)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			if(intermediateCookieAuthenticationScheme == null)
				throw new ArgumentNullException(nameof(intermediateCookieAuthenticationScheme));

			this.Logger.LogDebugIfEnabled($"Callback internal for authenticate-result and authentication-scheme {intermediateCookieAuthenticationScheme.ToStringRepresentation()} starting...");

			if(!authenticateResult.Succeeded)
			{
				this.Logger.LogErrorIfEnabled($"An authentication-attempt, using the scheme {intermediateCookieAuthenticationScheme.ToStringRepresentation()}, was made but failed. Available request-cookies: {string.Join(", ", this.Request.Cookies.Keys)}");

				throw new InvalidOperationException("Authentication error.", authenticateResult.Failure);
			}

			var returnUrl = this.ResolveAndValidateReturnUrl(authenticateResult.Properties?.Items[AuthenticationKeys.ReturnUrl]);

			var authenticationScheme = authenticateResult.Properties?.Items[AuthenticationKeys.Scheme];
			await this.ValidateAuthenticationSchemeForClientAsync(authenticationScheme, returnUrl);

			this.Logger.LogDebugIfEnabled($"Authentication-sheme = {authenticationScheme.ToStringRepresentation()}, claims received = {string.Join(", ", authenticateResult.Principal.Claims.Select(claim => claim.Type)).ToStringRepresentation()}.");

			var authenticationProperties = new AuthenticationProperties();
			var claims = new ClaimBuilderCollection();

			var claimsSelectionInformation = await this.GetClaimsSelectionInformationAsync(authenticateResult, authenticationScheme, returnUrl);

			if(claimsSelectionInformation.Handled)
			{
				this.Logger.LogDebugIfEnabled("Claims-selection handled.");

				// ReSharper disable LoopCanBeConvertedToQuery
				foreach(var claim in authenticateResult.Principal.Claims)
				{
					if(claimsSelectionInformation.ClaimTypes.Contains(claim.Type))
						claims.Add(new ClaimBuilder(claim));
				}
				// ReSharper restore LoopCanBeConvertedToQuery
			}
			else
			{
				this.Logger.LogDebugIfEnabled("Claims-selection NOT handled.");

				var decorators = (await this.Facade.DecorationLoader.GetCallbackDecoratorsAsync(authenticationScheme)).ToArray();

				if(decorators.Any())
				{
					this.Logger.LogDebugIfEnabled($"{decorators.Length} decorators will be used for authentication-scheme {authenticationScheme.ToStringRepresentation()}.");

					foreach(var decorator in decorators)
					{
						await decorator.DecorateAsync(authenticateResult, authenticationScheme, claims, authenticationProperties);

						this.Logger.LogDebugIfEnabled($"{decorator.GetType().FullName}.DecorateAsync claims = {string.Join(", ", claims.Select(claim => claim.Type)).ToStringRepresentation()}.");
					}
				}
				else
				{
					this.Logger.LogDebugIfEnabled($"There are no callback-decorators for authentication-scheme {authenticationScheme.ToStringRepresentation()}.");
				}

				await this.ResolveRequiredClaims(authenticateResult, authenticationScheme, claims);

				await this.ConvertToJwtClaimsAsync(claims);

				this.Logger.LogDebugIfEnabled($"Converted to jwt-claims = {string.Join(", ", claims.Select(claim => claim.Type)).ToStringRepresentation()}.");

				if(claimsSelectionInformation.Context != null)
				{
					var claimsPrincipal = await this.CreateClaimsPrincipalAsync(authenticateResult.Principal.Identity?.AuthenticationType, claims);
					var automaticSelectionIsPossible = await claimsSelectionInformation.Context.AutomaticSelectionIsPossibleAsync(claimsPrincipal);

					if(automaticSelectionIsPossible)
					{
						// todo: Fix automatic claims-selection.
					}

					await this.HttpContext.SignInAsync(this.IdentityServer.IntermediateCookieAuthenticationHandlers.ClaimsSelection.Name, claimsPrincipal, authenticateResult.Properties);

					await this.HttpContext.SignOutAsync(intermediateCookieAuthenticationScheme);

					return this.Redirect(claimsSelectionInformation.Context.Url.ToString());
				}
			}

			if(claimsSelectionInformation.Handled)
			{
				authenticationProperties.ExpiresUtc = authenticateResult.Properties?.ExpiresUtc;
				authenticationProperties.IssuedUtc = authenticateResult.Properties?.IssuedUtc;
				authenticationProperties.SetString(AuthenticationKeys.ClaimsSelectionClaimTypes, authenticateResult.Properties?.GetString(AuthenticationKeys.ClaimsSelectionClaimTypes));
			}

			var user = await this.ResolveUserAsync(authenticationScheme, claims);

			await this.ResolveAuthenticationLocally(authenticateResult, authenticationProperties, user);

			await this.HttpContext.SignInAsync(user, authenticationProperties);

			await this.HttpContext.SignOutAsync(intermediateCookieAuthenticationScheme);

			var authorizationRequest = await this.Facade.Interaction.GetAuthorizationContextAsync(returnUrl);

			await this.Facade.Events.RaiseAsync(new UserLoginSuccessEvent(user.IdentityProvider, user.ProviderUserId, user.SubjectId, user.DisplayName, true, authorizationRequest?.Client.ClientId));

			if(authorizationRequest != null && authorizationRequest.IsNativeClient())
				return await this.Redirect(returnUrl, this.Facade.IdentityServer.CurrentValue.SignOut.SecondsBeforeRedirectAfterSignOut);

			return this.Redirect(returnUrl);
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

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string authenticationType, IClaimBuilderCollection claims)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			return await Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(claims.Build(), authenticationType, claims.FindFirstNameClaim()?.Type, claims.FindFirst(ClaimTypes.Role, JwtClaimTypes.Role)?.Type)));
		}

		protected internal virtual async Task<ClaimsSelectionInformation> GetClaimsSelectionInformationAsync(AuthenticateResult authenticateResult, string authenticationScheme, string returnUrl)
		{
			if(authenticateResult == null)
				throw new ArgumentNullException(nameof(authenticateResult));

			var claimsSelectionInformation = new ClaimsSelectionInformation
			{
				Context = await this.Facade.ClaimsSelectionContextAccessor.GetAsync(authenticationScheme, returnUrl),
				Handled = await authenticateResult.GetClaimsSelectionHandledAsync()
			};

			foreach(var claimType in await authenticateResult.GetClaimsSelectionClaimTypesAsync())
			{
				claimsSelectionInformation.ClaimTypes.Add(claimType);
			}

			return claimsSelectionInformation;
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

		protected internal virtual async Task<ExtendedIdentityServerUser> ResolveUserAsync(string authenticationScheme, IClaimBuilderCollection claims)
		{
			if(authenticationScheme == null)
				throw new ArgumentNullException(nameof(authenticationScheme));

			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			claims = claims.Clone();

			var uniqueIdentifierClaim = claims.FindFirstUniqueIdentifierClaim();

			if(uniqueIdentifierClaim == null)
				throw new InvalidOperationException($"There is no unique-identifier-claim for authentication-scheme \"{authenticationScheme}\".");

			var uniqueIdentifier = uniqueIdentifierClaim.Value;
			claims.Remove(uniqueIdentifierClaim);

			var user = await this.Facade.Identity.ResolveUserAsync(claims, authenticationScheme, uniqueIdentifier);

			var nameClaim = claims.FindFirstNameClaim();
			var name = nameClaim?.Value;

			if(nameClaim != null)
				claims.Remove(nameClaim);

			return new ExtendedIdentityServerUser(user.Id)
			{
				AdditionalClaims = claims.Build(),
				DisplayName = name,
				IdentityProvider = authenticationScheme,
				ProviderUserId = uniqueIdentifier
			};
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