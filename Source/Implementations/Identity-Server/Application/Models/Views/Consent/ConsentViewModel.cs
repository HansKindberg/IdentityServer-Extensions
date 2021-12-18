using Duende.IdentityServer.Models;

namespace HansKindberg.IdentityServer.Application.Models.Views.Consent
{
	public class ConsentViewModel
	{
		#region Fields

		private ConsentForm _form;

		#endregion

		#region Properties

		public virtual bool AllowPersistent { get; set; }
		public virtual Client Client { get; set; }

		public virtual ConsentForm Form
		{
			get => this._form ??= new ConsentForm();
			set => this._form = value;
		}

		#endregion
	}
}