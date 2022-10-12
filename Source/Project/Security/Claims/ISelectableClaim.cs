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
		/// Builds the claim-collection this selectable claim generates.
		/// </summary>
		IClaimBuilderCollection Build();

		#endregion
	}
}