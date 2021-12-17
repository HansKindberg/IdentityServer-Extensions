using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Application.Models.Views.Grants
{
	public class GrantsViewModel
	{
		#region Properties

		public virtual IList<GrantViewModel> Grants { get; } = new List<GrantViewModel>();

		#endregion
	}
}