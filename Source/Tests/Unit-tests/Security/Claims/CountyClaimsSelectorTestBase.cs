using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;

namespace UnitTests.Security.Claims
{
	public abstract class CountyClaimsSelectorTestBase
	{
		#region Methods

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(IEnumerable<Claim> claims = null, bool isAuthenticated = true)
		{
			var claimsIdentity = new ClaimsIdentity(claims ?? Enumerable.Empty<Claim>(), isAuthenticated ? "Test" : null);
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			return await Task.FromResult(claimsPrincipal);
		}

		protected internal virtual async Task<IList<Commission>> CreateCommissionsAsync(byte numberOfCommissions)
		{
			var commissions = new List<Commission>();

			for(var i = 1; i <= numberOfCommissions; i++)
			{
				commissions.Add(new Commission
				{
					CommissionHsaId = $"{nameof(Commission.CommissionHsaId)}-{i}",
					CommissionName = $"{nameof(Commission.CommissionName)}-{i}",
					CommissionPurpose = $"{nameof(Commission.CommissionPurpose)}-{i}",
					EmployeeHsaId = $"{nameof(Commission.EmployeeHsaId)}-{i}",
					HealthCareProviderHsaId = $"{nameof(Commission.HealthCareProviderHsaId)}-{i}",
					HealthCareProviderName = $"{nameof(Commission.HealthCareProviderName)}-{i}"
				});
			}

			return await Task.FromResult(commissions);
		}

		#endregion
	}
}