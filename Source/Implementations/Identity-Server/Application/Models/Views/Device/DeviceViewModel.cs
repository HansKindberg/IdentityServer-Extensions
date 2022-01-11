namespace HansKindberg.IdentityServer.Application.Models.Views.Device
{
	public class DeviceViewModel
	{
		#region Properties

		public virtual string UserCode { get; set; }
		public virtual bool UserCodeIsInvalid { get; set; }

		#endregion
	}
}