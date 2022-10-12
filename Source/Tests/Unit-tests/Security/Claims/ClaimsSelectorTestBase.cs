using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Newtonsoft.Json;
using TestHelpers.Security.Claims;

namespace UnitTests.Security.Claims
{
	public abstract class ClaimsSelectorTestBase
	{
		#region Fields

		private string _resourceDirectoryPath;

		#endregion

		#region Properties

		protected internal virtual string ResourceDirectoryPath => this._resourceDirectoryPath ??= Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, @"Security\Claims\Resources", this.GetType().Name);

		#endregion

		#region Methods

		protected internal virtual async Task<IEnumerable<Claim>> CreateClaimsAsync(string claimsFileName, string commissionsFileName, string commissionsClaimType = "commissions")
		{
			return await this.CreateClaimsAsync(claimsFileName, await this.CreateCommissionsAsync(commissionsFileName), commissionsClaimType);
		}

		protected internal virtual async Task<IEnumerable<Claim>> CreateClaimsAsync(string claimsFileName, IEnumerable<Commission> commissions, string commissionsClaimType = "commissions")
		{
			var claimsFileContent = await File.ReadAllTextAsync(Path.Combine(this.ResourceDirectoryPath, $"{claimsFileName}.json"));
			var claimDictionaries = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(claimsFileContent);

			var claims = new List<Claim>();

			foreach(var claimDictionary in claimDictionaries ?? Enumerable.Empty<Dictionary<string, string>>())
			{
				foreach(var (key, value) in claimDictionary)
				{
					claims.Add(new Claim(key, value));
				}
			}

			var commissionsJson = JsonConvert.SerializeObject(commissions ?? Enumerable.Empty<Commission>(), Formatting.None);

			claims.Add(new Claim(commissionsClaimType, commissionsJson));

			return claims;
		}

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string claimsFileName, string commissionsFileName, string authenticationType = "Test", string commissionsClaimType = "commissions")
		{
			var claims = await this.CreateClaimsAsync(claimsFileName, commissionsFileName, commissionsClaimType);

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims, authenticationType);

			return claimsPrincipal;
		}

		protected internal virtual async Task<IList<Commission>> CreateCommissionsAsync(string commissionsFileName)
		{
			var commissionsFileContent = await File.ReadAllTextAsync(Path.Combine(this.ResourceDirectoryPath, $"{commissionsFileName}.json"));
			var commissions = JsonConvert.DeserializeObject<List<Commission>>(commissionsFileContent);

			return commissions;
		}

		protected internal virtual async Task<IList<Selection>> CreateSelectionsAsync(string commissionsFileName)
		{
			var commissions = await this.CreateCommissionsAsync(commissionsFileName);

			return commissions.Select(commission => new Selection { Commission = commission, EmployeeHsaId = commission.EmployeeHsaId }).ToList();
		}

		#endregion
	}
}