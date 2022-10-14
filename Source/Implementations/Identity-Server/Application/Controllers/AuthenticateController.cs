using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Duende.IdentityServer;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
		}

		#endregion

		#region Properties

		protected internal virtual ExtendedAuthenticationOptions Authentication { get; }

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

		#endregion
	}
}