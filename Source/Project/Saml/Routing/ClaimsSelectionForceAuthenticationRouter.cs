using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Saml.Routing
{
	/// <inheritdoc />
	public class ClaimsSelectionForceAuthenticationRouter : IForceAuthenticationRouter
	{
		#region Constructors

		public ClaimsSelectionForceAuthenticationRouter(IClaimsSelectionContextAccessor claimsSelectionContextAccessor)
		{
			this.ClaimsSelectionContextAccessor = claimsSelectionContextAccessor ?? throw new ArgumentNullException(nameof(claimsSelectionContextAccessor));
		}

		#endregion

		#region Properties

		protected internal virtual IClaimsSelectionContextAccessor ClaimsSelectionContextAccessor { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IActionResult> GetActionResultAsync(string returnUrl)
		{
			var claimsSelectionContext = await this.ClaimsSelectionContextAccessor.GetAsync(returnUrl);

			if(claimsSelectionContext == null)
				return null;

			var redirectResult = new RedirectResult(claimsSelectionContext.Url.ToString());

			return await Task.FromResult(redirectResult);
		}

		#endregion
	}
}