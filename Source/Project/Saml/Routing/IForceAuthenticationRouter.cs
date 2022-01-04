using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Saml.Routing
{
	public interface IForceAuthenticationRouter
	{
		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		Task<IActionResult> GetActionResultAsync(string returnUrl);

		#endregion
	}
}