namespace HansKindberg.IdentityServer.Configuration
{
	public class SignOutOptions
	{
		#region Properties

		public virtual bool AutomaticRedirectAfterSignOut { get; set; } = true;
		public virtual bool ConfirmSignOut { get; set; } = true;

		/// <summary>
		/// Enables IdP-initiated Single Logout (SLO). Requires SloEnabled = true to work.
		/// </summary>
		public virtual bool IdpInitiatedSloEnabled { get; set; } = true;

		public virtual byte SecondsBeforeRedirectAfterSignOut { get; set; } = 4;

		/// <summary>
		/// Enables Single Logout (SLO).
		/// </summary>
		public virtual bool SloEnabled { get; set; } = true;

		#endregion
	}
}