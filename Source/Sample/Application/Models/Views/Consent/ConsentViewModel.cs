using IdentityServer4.Models;

namespace Application.Models.Views.Consent
{
	public class ConsentViewModel
	{
		#region Fields

		private ConsentForm _form;

		#endregion

		#region Properties

		public virtual Client Client { get; set; }

		public virtual ConsentForm Form
		{
			get => this._form ??= new ConsentForm();
			set => this._form = value;
		}

		public virtual bool PersistenceEnabled { get; set; }

		#endregion
	}
}