using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace HansKindberg.RoleService.Models.Authorization
{
	public interface IRoleLoader
	{
		#region Methods

		Task<IEnumerable<string>> ListAsync(IPrincipal principal);

		#endregion
	}
}