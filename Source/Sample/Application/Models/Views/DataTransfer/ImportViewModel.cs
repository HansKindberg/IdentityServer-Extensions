using HansKindberg.IdentityServer.Data.Transferring;

namespace Application.Models.Views.DataTransfer
{
	public class ImportViewModel
	{
		#region Fields

		private ImportForm _form;

		#endregion

		#region Properties

		public virtual bool Confirmation { get; set; }

		public virtual ImportForm Form
		{
			get => this._form ??= new ImportForm();
			set => this._form = value;
		}

		public virtual IDataImportResult Result { get; set; }

		#endregion
	}
}