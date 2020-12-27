using System.ComponentModel.DataAnnotations;

namespace Application.Models.Views.Account
{
	public class SignInForm
	{
		#region Properties

		public virtual bool Cancel { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Password/Name", Prompt = "Password/Prompt")]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string Password { get; set; }

		[Display(Name = "Persistent/Name")]
		public virtual bool Persistent { get; set; }

		public virtual string ReturnUrl { get; set; }

		[Display(Name = "UserName/Name", Prompt = "UserName/Prompt")]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string UserName { get; set; }

		#endregion
	}
}