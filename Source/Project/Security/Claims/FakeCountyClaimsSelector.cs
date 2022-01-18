using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RegionOrebroLan.Web.Authentication.DirectoryServices;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	[Obsolete("Only for testing.")]
	public class FakeCountyClaimsSelector : CountyClaimsSelector
	{
		#region Constructors

		public FakeCountyClaimsSelector(IActiveDirectory activeDirectory, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(activeDirectory, httpContextAccessor, loggerFactory) { }

		#endregion

		#region Properties

		public virtual string AllCommissionsJson { get; set; }
		public virtual IList<string> AllEmployeeHsaIds { get; } = new List<string>();

		#endregion

		#region Methods

		protected internal override async Task<IList<Commission>> GetAllCommissionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			var allCommissionsJson = this.AllCommissionsJson ?? "[]";

			var commissions = JsonConvert.DeserializeObject<List<Commission>>(allCommissionsJson);

			return await Task.FromResult(commissions);
		}

		protected internal override async Task<IList<string>> GetAllEmployeeHsaIdsAsync(ClaimsPrincipal claimsPrincipal)
		{
			return await Task.FromResult(this.AllEmployeeHsaIds);
		}

		#endregion
	}
}