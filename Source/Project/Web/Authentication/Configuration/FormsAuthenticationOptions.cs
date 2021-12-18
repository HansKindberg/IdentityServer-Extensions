using System;

namespace HansKindberg.IdentityServer.Web.Authentication.Configuration
{
	public class FormsAuthenticationOptions
	{
		#region Properties

		public virtual bool AllowPersistent { get; set; } = true;
		public virtual TimeSpan Duration { get; set; } = TimeSpan.FromDays(30);

		#endregion
	}
}