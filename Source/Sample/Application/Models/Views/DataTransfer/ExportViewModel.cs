namespace Application.Models.Views.DataTransfer
{
	public class ExportViewModel
	{
		#region Fields

		private ExportForm _form;

		#endregion

		#region Properties

		public virtual ExportForm Form
		{
			get => this._form ??= new ExportForm();
			set => this._form = value;
		}

		#endregion
	}
}