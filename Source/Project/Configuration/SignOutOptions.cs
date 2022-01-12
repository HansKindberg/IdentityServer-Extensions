namespace HansKindberg.IdentityServer.Configuration
{
	public class SignOutOptions
	{
		#region Properties

		public virtual bool AutomaticRedirectAfterSignOut { get; set; } = true;
		public virtual bool ConfirmSignOut { get; set; } = true;

		/// <summary>
		/// Mode for single-sign-out / single-logout / SLO. What kind of SLO-modes that are supported: None, ClientInitiated, IdpInitiated or both ClientInitiated and IdpInitiated.
		/// </summary>
		public virtual SingleSignOutMode Mode { get; set; } = SingleSignOutMode.ClientInitiated | SingleSignOutMode.IdpInitiated;

		public virtual byte SecondsBeforeRedirectAfterSignOut { get; set; } = 4;

		#endregion
	}
}