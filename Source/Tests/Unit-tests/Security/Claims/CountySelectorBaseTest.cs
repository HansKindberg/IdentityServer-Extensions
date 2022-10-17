using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestHelpers.Security.Claims;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountySelectorBaseTest : ClaimsSelectorTestBase
	{
		#region Methods

		protected internal virtual async Task<CountySelectorBase<ISelectableClaim>> CreateCountySelectorBaseAsync()
		{
			var countySelectorBaseMock = new Mock<CountySelectorBase<ISelectableClaim>>(Mock.Of<ILoggerFactory>()) { CallBase = true };

			return await Task.FromResult(countySelectorBaseMock.Object);
		}

		[TestMethod]
		public async Task EmployeeHsaIdClaimType_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("hsa_identity", countySelectorBase.EmployeeHsaIdClaimType);
		}

		[TestMethod]
		public async Task GetEmployeeHsaIdsAsync_IfEmployeeHsaIdClaimTypeIsNull_ShouldReturnAnEmptySet()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();
			countySelectorBase.EmployeeHsaIdClaimType = null;
			Assert.IsNull(countySelectorBase.EmployeeHsaIdClaimType);

			var claims = new List<Claim>
			{
				new(string.Empty, string.Empty),
				new(string.Empty, string.Empty),
				new(string.Empty, string.Empty)
			};

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);

			var employeeHsaIds = await countySelectorBase.GetEmployeeHsaIdsAsync(claimsPrincipal);

			Assert.IsFalse(employeeHsaIds.Any());
		}

		[TestMethod]
		public async Task GetEmployeeHsaIdsAsync_ShouldBeCaseInsensitive()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			var employeeHsaIdClaimType = countySelectorBase.EmployeeHsaIdClaimType;

			var claims = new List<Claim>
			{
				new(employeeHsaIdClaimType, "A"),
				new(employeeHsaIdClaimType, "b"),
				new(employeeHsaIdClaimType, "c"),
				new(employeeHsaIdClaimType, "a")
			};

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);

			var employeeHsaIds = await countySelectorBase.GetEmployeeHsaIdsAsync(claimsPrincipal);

			Assert.AreEqual(3, employeeHsaIds.Count);
			Assert.AreEqual("A", employeeHsaIds.First());
			Assert.AreEqual("b", employeeHsaIds.ElementAt(1));
			Assert.AreEqual("c", employeeHsaIds.ElementAt(2));
		}

		[TestMethod]
		public async Task GetEmployeeHsaIdsAsync_ShouldNotContainDuplicates()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			var employeeHsaIdClaimType = countySelectorBase.EmployeeHsaIdClaimType;

			var claims = new List<Claim>
			{
				new(employeeHsaIdClaimType, "a"),
				new(employeeHsaIdClaimType, "b"),
				new(employeeHsaIdClaimType, "c"),
				new(employeeHsaIdClaimType, "a")
			};

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);

			var employeeHsaIds = await countySelectorBase.GetEmployeeHsaIdsAsync(claimsPrincipal);

			Assert.AreEqual(3, employeeHsaIds.Count);
			Assert.AreEqual("a", employeeHsaIds.First());
			Assert.AreEqual("b", employeeHsaIds.ElementAt(1));
			Assert.AreEqual("c", employeeHsaIds.ElementAt(2));
		}

		[TestMethod]
		public async Task GetSelectedEmployeeHsaIdAsync_ShouldWorkProperly()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new List<Claim>());

			var selectedEmployeeHsaId = await countySelectorBase.GetSelectedEmployeeHsaIdAsync(claimsPrincipal);

			Assert.IsNull(selectedEmployeeHsaId);

			var claims = new List<Claim>
			{
				new(countySelectorBase.SelectedEmployeeHsaIdClaimType, "1")
			};

			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);

			selectedEmployeeHsaId = await countySelectorBase.GetSelectedEmployeeHsaIdAsync(claimsPrincipal);

			Assert.AreEqual("1", selectedEmployeeHsaId);

			claims.Add(new Claim(countySelectorBase.SelectedEmployeeHsaIdClaimType, "2"));

			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);

			selectedEmployeeHsaId = await countySelectorBase.GetSelectedEmployeeHsaIdAsync(claimsPrincipal);

			Assert.AreEqual("1", selectedEmployeeHsaId);
		}

		[TestMethod]
		public async Task SelectedClaimTypePrefix_ShouldAffectSelectedEmployeeHsaIdClaimType()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();
			Assert.AreEqual("selected_", countySelectorBase.SelectedClaimTypePrefix);
			Assert.AreEqual("selected_employeeHsaId", countySelectorBase.SelectedEmployeeHsaIdClaimType);

			countySelectorBase.SelectedClaimTypePrefix = "test_";
			Assert.AreEqual("test_", countySelectorBase.SelectedClaimTypePrefix);
			Assert.AreEqual("test_employeeHsaId", countySelectorBase.SelectedEmployeeHsaIdClaimType);

			countySelectorBase.SelectedClaimTypePrefix = string.Empty;
			Assert.AreEqual(string.Empty, countySelectorBase.SelectedClaimTypePrefix);
			Assert.AreEqual("employeeHsaId", countySelectorBase.SelectedEmployeeHsaIdClaimType);

			countySelectorBase.SelectedClaimTypePrefix = null;
			Assert.IsNull(countySelectorBase.SelectedClaimTypePrefix);
			Assert.AreEqual("employeeHsaId", countySelectorBase.SelectedEmployeeHsaIdClaimType);
		}

		[TestMethod]
		public async Task SelectedClaimTypePrefix_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("selected_", countySelectorBase.SelectedClaimTypePrefix);
		}

		[TestMethod]
		public async Task SelectedEmployeeHsaIdClaimType_ShouldWorkProperly()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("selected_employeeHsaId", countySelectorBase.SelectedEmployeeHsaIdClaimType);
		}

		#endregion
	}
}