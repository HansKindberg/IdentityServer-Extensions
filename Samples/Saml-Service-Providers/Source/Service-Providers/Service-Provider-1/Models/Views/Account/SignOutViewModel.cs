namespace Application.Models.Views.Account
{
	public class SignOutViewModel
	{
		#region Fields

		private SignOutForm _form;

		#endregion

		#region Properties

		public virtual SignOutForm Form
		{
			get => this._form ??= new SignOutForm();
			set => this._form = value;
		}

		#endregion
	}
}