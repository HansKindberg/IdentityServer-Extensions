using System;
using System.Threading.Tasks;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public abstract class AuthenticateControllerBase : SiteController
	{
		#region Constructors

		protected AuthenticateControllerBase(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

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

		#endregion
	}
}