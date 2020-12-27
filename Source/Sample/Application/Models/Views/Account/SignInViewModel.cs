using System.Collections.Generic;
using HansKindberg.IdentityServer.Web.Authentication.Configuration;
using RegionOrebroLan.Web.Authentication;

namespace Application.Models.Views.Account
{
	public class SignInViewModel
	{
		#region Fields

		private SignInForm _form;

		#endregion

		#region Properties

		public virtual IList<IAuthenticationScheme> AuthenticationSchemes { get; } = new List<IAuthenticationScheme>();

		public virtual SignInForm Form
		{
			get => this._form ??= new SignInForm();
			set => this._form = value;
		}

		public virtual FormsAuthenticationOptions FormsAuthentication { get; } = new FormsAuthenticationOptions();
		public virtual bool FormsAuthenticationEnabled { get; set; }

		#endregion
	}
}