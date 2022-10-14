using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
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
		#region Fields

		private const string _group = "FakeCounty";

		#endregion

		#region Constructors

		public FakeClaimBasedCountySelector(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Properties

		public virtual string CommissionsJson { get; set; }
		public override string Group { get; set; } = _group;

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