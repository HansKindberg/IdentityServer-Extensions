using System.Collections.Generic;
using RegionOrebroLan.Web.Authentication;

namespace Application.Models.Views.Diagnostics
{
	public class AuthenticationSchemeViewModel
	{
		#region Properties

		public virtual bool AuthenticationSchemesMissing { get; set; }
		public virtual IDictionary<IAuthenticationScheme, string> Items { get; } = new Dictionary<IAuthenticationScheme, string>();

		#endregion
	}
}