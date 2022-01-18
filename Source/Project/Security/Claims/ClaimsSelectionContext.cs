using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class ClaimsSelectionContext : IClaimsSelectionContext
	{
		#region Properties

		public virtual string AuthenticationScheme { get; set; }
		IEnumerable<IClaimsSelector> IClaimsSelectionContext.Selectors => this.Selectors;
		public virtual IList<IClaimsSelector> Selectors { get; } = new List<IClaimsSelector>();
		public virtual Uri Url { get; set; }

		#endregion
	}
}