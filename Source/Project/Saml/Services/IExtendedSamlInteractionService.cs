using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rsk.Saml.Services;

namespace HansKindberg.IdentityServer.Saml.Services
{
	public interface IExtendedSamlInteractionService : ISamlInteractionService
	{
		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		Task<IActionResult> GetForceAuthenticationActionResultAsync(string returnUrl);

		#endregion
	}
}