using System.ComponentModel.DataAnnotations;

namespace Application.Models.Views.DataTransfer
{
	public class ImportForm
	{
		#region Properties

		[Display(Description = "DeleteAllOthers-Description", Name = "DeleteAllOthers")]
		public virtual bool DeleteAllOthers { get; set; }

		[Display(Description = "Files-Description", Name = "Files")]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual string Files { get; set; }

		[Display(Description = "VerifyOnly-Description", Name = "VerifyOnly")]
		public virtual bool VerifyOnly { get; set; }

		#endregion
	}
}