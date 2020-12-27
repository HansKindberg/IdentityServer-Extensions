using System.Collections.Generic;

namespace Application.Models.Views.Diagnostics
{
	public class ConfigurationViewModel
	{
		#region Properties

		public virtual IList<ConfigurationProviderViewModel> ConfigurationProviders { get; } = new List<ConfigurationProviderViewModel>();

		#endregion
	}
}