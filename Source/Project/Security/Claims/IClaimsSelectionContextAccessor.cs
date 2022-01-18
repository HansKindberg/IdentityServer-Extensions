using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectionContextAccessor
	{
		#region Properties

		IClaimsSelectionContext ClaimsSelectionContext { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		Task<IClaimsSelectionContext> GetAsync(string returnUrl);

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		Task<IClaimsSelectionContext> GetAsync(string authenticationScheme, string returnUrl);

		#endregion
	}
}