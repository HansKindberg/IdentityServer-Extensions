using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.IdentityServer.Web.Authentication
{
	public interface IMutualTlsService
	{
		#region Methods

		/// <summary>
		/// The issuer-origin for the IdentityServer instance. Eg. scheme://hostname or scheme://hostname:port.
		/// </summary>
		Task<string> GetIssuerOriginAsync();

		/// <summary>
		/// The mTLS-origin for the IdentityServer instance. Eg. scheme://hostname or scheme://hostname:port.
		/// </summary>
		Task<string> GetMtlsOriginAsync();

		/// <summary>
		/// If the http-request is a request to the particular mTLS-domain.
		/// </summary>
		Task<bool> IsMtlsDomainRequestAsync(HttpRequest httpRequest);

		#endregion
	}
}