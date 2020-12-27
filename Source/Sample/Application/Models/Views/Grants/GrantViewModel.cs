using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace Application.Models.Views.Grants
{
	public class GrantViewModel
	{
		#region Properties

		public virtual IList<ApiScope> ApiScopes { get; } = new List<ApiScope>();
		public virtual Client Client { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual string Description { get; set; }
		public virtual DateTime? Expiration { get; set; }
		public virtual IList<IdentityResource> IdentityResources { get; } = new List<IdentityResource>();

		#endregion
	}
}