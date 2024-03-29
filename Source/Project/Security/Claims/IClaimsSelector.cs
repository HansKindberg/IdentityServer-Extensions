using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelector
	{
		#region Properties

		/// <summary>
		/// The key for this claims-selector. Each claims-selector must have a unique key.
		/// </summary>
		string Key { get; }

		bool SelectionRequired { get; }

		#endregion

		#region Methods

		Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, IClaimsSelectionResult selectionResult);
		Task InitializeAsync(IConfiguration optionsConfiguration);
		Task<IClaimsSelectionResult> SelectAsync(ClaimsPrincipal claimsPrincipal, IDictionary<string, string> selections);

		#endregion
	}
}