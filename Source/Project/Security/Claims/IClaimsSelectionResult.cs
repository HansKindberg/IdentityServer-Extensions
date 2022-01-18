using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	public interface IClaimsSelectionResult
	{
		#region Properties

		bool Complete { get; }
		IDictionary<string, IList<ISelectableClaim>> Selectables { get; }
		IClaimsSelector Selector { get; }

		#endregion
	}
}