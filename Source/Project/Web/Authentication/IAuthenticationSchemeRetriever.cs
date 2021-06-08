using System.Collections.Generic;
using System.Threading.Tasks;
using RegionOrebroLan.Web.Authentication;

namespace HansKindberg.IdentityServer.Web.Authentication
{
	public interface IAuthenticationSchemeRetriever
	{
		#region Methods

		Task<IAuthenticationScheme> GetAsync(string name);
		Task<IEnumerable<IAuthenticationScheme>> ListAsync();

		#endregion
	}
}