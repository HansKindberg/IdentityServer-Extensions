using System;

namespace HansKindberg.IdentityServer.Web.Authentication.Configuration
{
	public class FormsAuthenticationOptions
	{
		#region Properties

		public virtual TimeSpan Duration { get; set; } = TimeSpan.FromDays(30);
		public virtual bool Persistent { get; set; } = true;

		#endregion
	}
}