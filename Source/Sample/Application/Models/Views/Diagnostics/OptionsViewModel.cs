using System;
using System.Collections.Generic;

namespace Application.Models.Views.Diagnostics
{
	public class OptionsViewModel
	{
		#region Properties

		public virtual IDictionary<Type, string> Options { get; } = new Dictionary<Type, string>();

		#endregion
	}
}