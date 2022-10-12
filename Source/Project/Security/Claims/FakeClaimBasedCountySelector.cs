using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// Faked selector for selecting county-claims.
	/// </summary>
	/// <inheritdoc />
	[Obsolete("Only for testing.")]
	public class FakeClaimBasedCountySelector : CountySelectorBase
	{
		#region Constructors

		public FakeClaimBasedCountySelector(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(httpContextAccessor, loggerFactory) { }

		#endregion

		#region Properties

		public virtual string CommissionsJson { get; set; }

		#endregion

		#region Methods

		protected internal override async Task<IList<Selection>> GetSelectionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			var commissionsJson = this.CommissionsJson ?? "[]";
			var commissions = JsonConvert.DeserializeObject<List<Commission>>(commissionsJson) ?? Enumerable.Empty<Commission>().ToList();

			var selections = commissions.Select(commission => new Selection { Commission = commission, EmployeeHsaId = commission.EmployeeHsaId }).ToList();

			return await Task.FromResult(selections).ConfigureAwait(false);
		}

		#endregion
	}
}