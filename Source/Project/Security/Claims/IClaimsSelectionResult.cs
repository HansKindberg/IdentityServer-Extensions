using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectionResult
	{
		#region Properties

		IDictionary<string, IList<ISelectableClaim>> Selectables { get; }
		IClaimsSelector Selector { get; }

		#endregion
	}
}