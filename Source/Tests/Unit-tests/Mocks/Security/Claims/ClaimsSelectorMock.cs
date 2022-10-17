using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.Extensions.Configuration;
using RegionOrebroLan.Security.Claims;

namespace UnitTests.Mocks.Security.Claims
{
	public class ClaimsSelectorMock : IClaimsSelector
	{
		#region Properties

		public virtual IDictionary<string, IClaimBuilderCollection> Claims { get; } = new Dictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase);
		public virtual ClaimsSelectionResultMock ClaimsSelectionResult { get; set; } = new();
		public virtual string Key { get; set; }
		public virtual bool SelectionRequired { get; set; }

		#endregion

		#region Methods

		public virtual async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, IClaimsSelectionResult selectionResult)
		{
			return await Task.FromResult(this.Claims);
		}

		public virtual async Task InitializeAsync(IConfiguration optionsConfiguration)
		{
			await Task.CompletedTask;
		}

		public virtual async Task<IClaimsSelectionResult> SelectAsync(ClaimsPrincipal claimsPrincipal, IDictionary<string, string> selections)
		{
			return await Task.FromResult(this.ClaimsSelectionResult);
		}

		#endregion
	}
}