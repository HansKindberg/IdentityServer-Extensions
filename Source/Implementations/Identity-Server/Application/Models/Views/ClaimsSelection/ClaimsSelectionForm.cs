using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection
{
	public class ClaimsSelectionForm
	{
		#region Properties

		public virtual string AuthenticationScheme { get; set; }
		public virtual bool Cancel { get; set; }
		public virtual IDictionary<string, ClaimsSelectionGroup> Groups { get; } = new Dictionary<string, ClaimsSelectionGroup>(StringComparer.OrdinalIgnoreCase);
		public virtual bool RequiredSelectionsSelected { get; set; }

		#endregion
	}
}