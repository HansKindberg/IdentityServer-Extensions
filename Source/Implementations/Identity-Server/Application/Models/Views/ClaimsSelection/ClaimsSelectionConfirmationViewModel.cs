using HansKindberg.IdentityServer.Application.Models.Views.Shared;

namespace HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection
{
	public class ClaimsSelectionConfirmationViewModel : SingleSignOutViewModel
	{
		#region Properties

		public virtual string AuthenticationScheme { get; set; }

		#endregion
	}
}