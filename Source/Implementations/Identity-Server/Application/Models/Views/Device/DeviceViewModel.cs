namespace HansKindberg.IdentityServer.Application.Models.Views.Device
{
	public class DeviceViewModel
	{
		#region Fields

		private DeviceForm _form;

		#endregion

		#region Properties

		public virtual DeviceForm Form
		{
			get => this._form ??= new DeviceForm();
			set => this._form = value;
		}

		#endregion
	}
}