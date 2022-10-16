using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestHelpers.Mocks.Logging;
using TestHelpers.Security.Claims;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class FakeClaimBasedCountySelectorTest : ClaimsSelectorTestBase
	{
		#region Methods

		protected internal virtual async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(string claimsFileName, string authenticationType = "Test")
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

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims, authenticationType);

			return claimsPrincipal;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		protected internal virtual async Task<FakeClaimBasedCountySelector> CreateFakeClaimBasedCountySelectorAsync(string commissionsFileName, ILoggerFactory loggerFactory)
		{
			var fakeClaimBasedCountySelector = new FakeClaimBasedCountySelector(loggerFactory);

			var commissionsFileContent = await File.ReadAllTextAsync(Path.Combine(this.ResourceDirectoryPath, $"{commissionsFileName}.json"));
			var commissions = JsonConvert.DeserializeObject<List<Commission>>(commissionsFileContent);
			var commissionsJson = JsonConvert.SerializeObject(commissions ?? Enumerable.Empty<Commission>(), Formatting.None);

			fakeClaimBasedCountySelector.CommissionsJson = commissionsJson;

			return fakeClaimBasedCountySelector;
		}
#pragma warning restore CS0618 // Type or member is obsolete

		[TestMethod]
		public async Task GetClaimsAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");
				var fakeClaimBasedCountySelector = await this.CreateFakeClaimBasedCountySelectorAsync("Commissions-1", loggerFactory);

				var result = await fakeClaimBasedCountySelector.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				var claims = await fakeClaimBasedCountySelector.GetClaimsAsync(claimsPrincipal, result);

				Assert.AreEqual(11, claims.Count);

				var (key, value) = claims.ElementAt(0);
				Assert.AreEqual("selected_commissionHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_commissionHsaId", value.First().Type);
				Assert.AreEqual("TEST123456-c002", value.First().Value);

				(key, value) = claims.ElementAt(1);
				Assert.AreEqual("selected_commissionName", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_commissionName", value.First().Type);

				(key, value) = claims.ElementAt(2);
				Assert.AreEqual("selected_commissionPurpose", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_commissionPurpose", value.First().Type);

				(key, value) = claims.ElementAt(3);
				Assert.AreEqual("selected_commissionRight", key);
				Assert.AreEqual(3, value.Count);
				Assert.AreEqual("selected_commissionRight", value.First().Type);
				Assert.AreEqual("selected_commissionRight", value.ElementAt(1).Type);
				Assert.AreEqual("selected_commissionRight", value.ElementAt(2).Type);

				(key, value) = claims.ElementAt(4);
				Assert.AreEqual("selected_employeeHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_employeeHsaId", value.First().Type);
				Assert.AreEqual("TEST123456-e001", value.First().Value);

				(key, value) = claims.ElementAt(5);
				Assert.AreEqual("selected_healthCareProviderHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderHsaId", value.First().Type);

				(key, value) = claims.ElementAt(6);
				Assert.AreEqual("selected_healthCareProviderName", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderName", value.First().Type);

				(key, value) = claims.ElementAt(7);
				Assert.AreEqual("selected_healthCareProviderOrgNo", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderOrgNo", value.First().Type);

				(key, value) = claims.ElementAt(8);
				Assert.AreEqual("selected_healthCareUnitHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitHsaId", value.First().Type);

				(key, value) = claims.ElementAt(9);
				Assert.AreEqual("selected_healthCareUnitName", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitName", value.First().Type);

				(key, value) = claims.ElementAt(10);
				Assert.AreEqual("selected_healthCareUnitStartDate", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitStartDate", value.First().Type);
			}
		}

		[TestMethod]
		public async Task SelectAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");
				var fakeClaimBasedCountySelector = await this.CreateFakeClaimBasedCountySelectorAsync("Commissions-1", loggerFactory);

				var result = await fakeClaimBasedCountySelector.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[fakeClaimBasedCountySelector.Group].Count);
				Assert.IsTrue(result.Selectables[fakeClaimBasedCountySelector.Group].ElementAt(1).Selected);
			}
		}

		#endregion
	}
}