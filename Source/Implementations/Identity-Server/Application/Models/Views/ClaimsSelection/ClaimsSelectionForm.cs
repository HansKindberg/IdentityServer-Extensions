using System;
using System.Collections.Generic;
using HansKindberg.IdentityServer.Security.Claims;

namespace HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection
{
	public class ClaimsSelectionForm
	{
		#region Properties

		public virtual string AuthenticationScheme { get; set; }
		public virtual bool Cancel { get; set; }
		public virtual IDictionary<string, IList<ISelectableClaim>> SelectableClaims { get; } = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}