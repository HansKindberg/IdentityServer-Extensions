using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountySelectableClaimTest : ClaimsSelectorTestBase
	{
		#region Methods

		[TestMethod]
		[SuppressMessage("Style", "JSON002:Probable JSON string detected")]
		public async Task Build_Test()
		{
			var selection = (await this.CreateSelectionsAsync("Commissions-Only-One")).First();

			var countySelectableClaim = new CountySelectableClaim("selected_", "Test", false, selection);

			var claimsDictionary = countySelectableClaim.Build();

			Assert.AreEqual(18, claimsDictionary.Count);

			var (key, claims) = claimsDictionary.ElementAt(0);
			Assert.AreEqual("selected_commissionHsaId", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("TEST123456-c003", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(1);
			Assert.AreEqual("selected_commissionName", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("Commission 3", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(2);
			Assert.AreEqual("selected_commissionPurpose", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("Purpose 3", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(3);
			Assert.AreEqual("selected_commissionRight", key);
			Assert.AreEqual(9, claims.Count);
			Assert.AreEqual("{\"Activity\":\"AB\",\"InformationClass\":\"a1\",\"Scope\":\"AB\"}", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(4);
			Assert.AreEqual("selected_employeeHsaId", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("TEST123456-e001", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(5);
			Assert.AreEqual("selected_givenName", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(6);
			Assert.AreEqual("selected_healthCareProviderHsaId", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("TEST123456-hp03", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(7);
			Assert.AreEqual("selected_healthCareProviderName", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("Healthcare provider 3", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(8);
			Assert.AreEqual("selected_healthCareProviderOrgNo", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("TEST123456", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(9);
			Assert.AreEqual("selected_healthCareUnitHsaId", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("TEST123456-hu03", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(10);
			Assert.AreEqual("selected_healthCareUnitName", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("Healthcare unit 3", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(11);
			Assert.AreEqual("selected_healthCareUnitStartDate", key);
			Assert.AreEqual(1, claims.Count);
			Assert.AreEqual("{\"Day\":1,\"Month\":1,\"Year\":2000}", claims.ElementAt(0).Value);

			(key, claims) = claimsDictionary.ElementAt(12);
			Assert.AreEqual("selected_mail", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(13);
			Assert.AreEqual("selected_paTitleCode", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(14);
			Assert.AreEqual("selected_personalIdentityNumber", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(15);
			Assert.AreEqual("selected_personalPrescriptionCode", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(16);
			Assert.AreEqual("selected_surname", key);
			Assert.AreEqual(0, claims.Count);

			(key, claims) = claimsDictionary.ElementAt(17);
			Assert.AreEqual("selected_systemRole", key);
			Assert.AreEqual(0, claims.Count);
		}

		#endregion
	}
}