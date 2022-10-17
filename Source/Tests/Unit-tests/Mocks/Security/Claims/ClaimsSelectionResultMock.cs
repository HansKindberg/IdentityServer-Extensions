using System;
using System.Collections.Generic;
using HansKindberg.IdentityServer.Security.Claims;

namespace UnitTests.Mocks.Security.Claims
{
	public class ClaimsSelectionResultMock : IClaimsSelectionResult
	{
		#region Properties

		public virtual IDictionary<string, IList<ISelectableClaim>> Selectables { get; } = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase);
		public virtual IClaimsSelector Selector { get; set; }

		#endregion
	}
}