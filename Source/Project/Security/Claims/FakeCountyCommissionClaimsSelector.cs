using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	[Obsolete("Only for testing.")]
	public class FakeCountyCommissionClaimsSelector : CountyCommissionClaimsSelector
	{
		#region Constructors

		public FakeCountyCommissionClaimsSelector(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(httpContextAccessor, loggerFactory) { }

		#endregion

		#region Properties

		public virtual string AllCommissionsJson { get; set; }

		#endregion

		#region Methods

		protected internal override async Task<IList<Commission>> GetAllCommissionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			var allCommissionsJson = this.AllCommissionsJson ?? "[]";

			var commissions = JsonConvert.DeserializeObject<List<Commission>>(allCommissionsJson);

			return await Task.FromResult(commissions).ConfigureAwait(false);
		}

		#endregion
	}
}