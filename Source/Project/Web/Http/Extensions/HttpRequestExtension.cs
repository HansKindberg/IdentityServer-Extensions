using System;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.IdentityServer.Web.Http.Extensions
{
	public static class HttpRequestExtension
	{
		#region Methods

		/// <summary>
		/// The origin for the http-request. The http-request itself as origin. The request-headers are not involved. Eg. scheme://hostname or scheme://hostname:port.
		/// </summary>
		public static string Origin(this HttpRequest httpRequest)
		{
			if(httpRequest == null)
				throw new ArgumentNullException(nameof(httpRequest));

			return $"{httpRequest.Scheme}{Uri.SchemeDelimiter}{httpRequest.Host}";
		}

		#endregion
	}
}