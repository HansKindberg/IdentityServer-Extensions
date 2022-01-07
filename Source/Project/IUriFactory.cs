using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace HansKindberg.IdentityServer
{
	public interface IUriFactory
	{
		#region Methods

		Task<Uri> CreateRelativeAsync(string path, string query);
		Task<Uri> CreateRelativeAsync(IEnumerable<string> segments, UriFactoryQueryMode uriFactoryQueryMode = UriFactoryQueryMode.UiLocales);
		Task<Uri> CreateRelativeAsync(CultureInfo culture, bool includeContextPath = true, UriFactoryQueryMode uriFactoryQueryMode = UriFactoryQueryMode.UiLocales);

		#endregion
	}
}