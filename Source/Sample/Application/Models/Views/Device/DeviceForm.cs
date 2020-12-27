using System.ComponentModel.DataAnnotations;

namespace Application.Models.Views.Device
{
	public class DeviceForm
	{
		#region Properties

		[Display(Name = "UserCode/Name", Prompt = "UserCode/Prompt")]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string UserCode { get; set; }

		#endregion
	}
}