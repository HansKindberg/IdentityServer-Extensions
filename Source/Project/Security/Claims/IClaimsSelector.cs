using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelector
	{
		#region Properties

		bool SelectionRequired { get; }

		#endregion

		#region Methods

		Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(IClaimsSelectionResult selectionResult);
		Task InitializeAsync(IConfiguration optionsConfiguration);
		Task<IClaimsSelectionResult> SelectAsync(IDictionary<string, string> selections);

		#endregion
	}
}