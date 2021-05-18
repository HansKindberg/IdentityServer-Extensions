using System;
using System.Collections.Generic;

namespace Application.Models.Views.Diagnostics
{
	public class CookieInformationViewModel
	{
		#region Properties

		public virtual IDictionary<string, int> Cookies { get; } = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}