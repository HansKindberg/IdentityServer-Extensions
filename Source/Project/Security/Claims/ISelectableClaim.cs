using System.Collections.Generic;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface ISelectableClaim
	{
		#region Properties

		IReadOnlyDictionary<string, string> Details { get; }
		string Id { get; }
		bool Selected { get; }
		string Text { get; }
		string Value { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Builds a dictionary where the key is the claim-type and the value is a collection of claims for that claim-type.
		/// The dictionary should contain a key for all claim-types involved for this selectable claim even if the value is an empty collection of claims.
		/// By delivering all claim-types as keys it is possible to do cleanup/removal of previously selected claims even if the new selection contains no claims of that claim-type.
		/// </summary>
		IDictionary<string, IClaimBuilderCollection> Build();

		#endregion
	}
}