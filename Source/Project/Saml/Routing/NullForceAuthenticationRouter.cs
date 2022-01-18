using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Saml.Routing
{
	/// <inheritdoc />
	public class NullForceAuthenticationRouter : IForceAuthenticationRouter
	{
		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IActionResult> GetActionResultAsync(string returnUrl)
		{
			return await Task.FromResult((IActionResult)null);
		}

		#endregion
	}
}