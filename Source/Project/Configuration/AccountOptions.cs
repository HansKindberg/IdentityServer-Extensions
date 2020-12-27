using HansKindberg.IdentityServer.Web.Authentication.Configuration;

namespace HansKindberg.IdentityServer.Configuration
{
	public class AccountOptions
	{
		#region Properties

		public virtual bool AutomaticRedirectAfterSignOut { get; set; } = true;
		public virtual bool ConfirmSignOut { get; set; } = true;
		public virtual FormsAuthenticationOptions FormsAuthentication { get; set; } = new FormsAuthenticationOptions();

		#endregion
	}
}