namespace HansKindberg.IdentityServer.Configuration
{
	public class SignOutOptions
	{
		#region Properties

		public virtual bool AutomaticRedirectAfterSignOut { get; set; } = true;
		public virtual bool ConfirmSignOut { get; set; } = true;
		public virtual byte SecondsBeforeRedirectAfterSignOut { get; set; } = 4;

		#endregion
	}
}