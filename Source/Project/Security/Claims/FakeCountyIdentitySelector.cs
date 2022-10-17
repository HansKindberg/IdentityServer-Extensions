using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// Faked selector for selecting county-identity.
	/// </summary>
	/// <inheritdoc />
	[Obsolete("Only for testing.")]
	public class FakeCountyIdentitySelector : CountyIdentitySelectorBase
	{
		#region Constructors

		public FakeCountyIdentitySelector(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Properties

		public virtual string IdentitiesJson { get; set; }

		#endregion

		#region Methods

		protected internal override async Task<IList<string>> GetIdentitiesAsync(ClaimsPrincipal claimsPrincipal)
		{
			var identitiesJson = this.IdentitiesJson ?? "[]";
			var identities = JsonConvert.DeserializeObject<List<string>>(identitiesJson) ?? Enumerable.Empty<string>().ToList();

			return await Task.FromResult(identities).ConfigureAwait(false);
		}

		#endregion
	}
}