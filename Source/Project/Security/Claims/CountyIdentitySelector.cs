using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// Select county-identity.
	/// </summary>
	/// <inheritdoc />
	public class CountyIdentitySelector : CountyIdentitySelectorBase
	{
		#region Constructors

		public CountyIdentitySelector(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		protected internal override async Task<IList<string>> GetIdentitiesAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var employeeHsaIds = await this.GetEmployeeHsaIdsAsync(claimsPrincipal).ConfigureAwait(false);

			return employeeHsaIds.ToList();
		}

		#endregion
	}
}