using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.RoleService.Models.Authorization
{
	public interface IRoleResolver
	{
		#region Properties

		IEnumerable<IRoleLoader> Loaders { get; }

		#endregion

		#region Methods

		Task<IEnumerable<string>> ResolveAsync(HttpContext httpContext);

		#endregion
	}
}