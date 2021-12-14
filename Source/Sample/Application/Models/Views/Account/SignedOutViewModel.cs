using Application.Models.Views.Shared;

namespace Application.Models.Views.Account
{
	public class SignedOutViewModel : RedirectViewModel
	{
		#region Properties

		public virtual bool AutomaticRedirect { get; set; }
		public virtual string Client { get; set; }
		public virtual string IframeUrl { get; set; }
		public virtual string SamlIframeUrl { get; set; }

		#endregion
	}
}