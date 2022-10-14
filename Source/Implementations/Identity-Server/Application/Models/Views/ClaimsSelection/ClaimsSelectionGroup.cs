using System.Collections.Generic;
using HansKindberg.IdentityServer.Security.Claims;

namespace HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection
{
	public class ClaimsSelectionGroup
	{
		#region Properties

		public virtual IList<ISelectableClaim> SelectableClaims { get; } = new List<ISelectableClaim>();
		public virtual bool SelectionRequired { get; set; }

		#endregion
	}
}