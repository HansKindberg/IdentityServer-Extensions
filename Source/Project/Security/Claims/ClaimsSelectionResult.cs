using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class ClaimsSelectionResult : IClaimsSelectionResult
	{
		#region Constructors

		public ClaimsSelectionResult(IClaimsSelector selector)
		{
			this.Selector = selector ?? throw new ArgumentNullException(nameof(selector));
		}

		#endregion

		#region Properties

		public virtual IDictionary<string, IList<ISelectableClaim>> Selectables { get; } = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase);
		public virtual IClaimsSelector Selector { get; }

		#endregion
	}
}