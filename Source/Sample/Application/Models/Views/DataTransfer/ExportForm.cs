using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Application.Models.Views.DataTransfer
{
	public class ExportForm
	{
		#region Properties

		public virtual IList<SelectListItem> TypeList { get; } = new List<SelectListItem>();

		[Display(Description = "Types-Description", Name = "Types")]
		[Required(ErrorMessage = "\"{0}\" is required.")]
		public virtual IEnumerable<string> Types { get; set; }

		#endregion
	}
}