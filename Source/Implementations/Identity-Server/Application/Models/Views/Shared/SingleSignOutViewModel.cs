namespace HansKindberg.IdentityServer.Application.Models.Views.Shared
{
	public abstract class SingleSignOutViewModel : RedirectViewModel
	{
		#region Properties

		public virtual bool AutomaticRedirect { get; set; }
		public virtual string Client { get; set; }
		public virtual string IframeUrl { get; set; }
		public virtual string SamlIframeUrl { get; set; }

		#endregion
	}
}