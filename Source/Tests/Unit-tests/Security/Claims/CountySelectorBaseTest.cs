using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
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

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync()
		{
			return await this.CreateCountySelectorBaseAsync(Mock.Of<IHttpContextAccessor>(), Mock.Of<ILoggerFactory>(), Enumerable.Empty<Selection>().ToList());
		}

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync(ClaimsPrincipal claimsPrincipal, IList<Selection> selections = null)
		{
			var httpContextMock = new Mock<HttpContext>();
			httpContextMock.Setup(httpContext => httpContext.User).Returns(claimsPrincipal);
			var httpContext = httpContextMock.Object;

			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);
			var httpContextAccessor = httpContextAccessorMock.Object;

			return await this.CreateCountySelectorBaseAsync(httpContextAccessor, Mock.Of<ILoggerFactory>(), selections ?? Enumerable.Empty<Selection>().ToList());
		}

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IList<Selection> selections)
		{
			var countySelectorBaseMock = new Mock<CountySelectorBase>(httpContextAccessor, loggerFactory) { CallBase = true };

			countySelectorBaseMock.Setup(countySelectorBase => countySelectorBase.GetSelectionsAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(selections));

			return await Task.FromResult(countySelectorBaseMock.Object);
		}

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync(string claimsFileName, string commissionsFileName, string authenticationType = "Test", string commissionsClaimType = "commissions")
		{
			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claimsFileName, commissionsFileName, authenticationType, commissionsClaimType);

			var selections = await this.CreateSelectionsAsync(commissionsFileName);

			var countySelectorBase = await this.CreateCountySelectorBaseAsync(claimsPrincipal, selections);

			return countySelectorBase;
		}

		[TestMethod]
		public async Task EmployeeHsaIdClaimType_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("hsa_identity", countySelectorBase.EmployeeHsaIdClaimType);
		}

		[TestMethod]
		public async Task GetClaimsAsync_ShouldWorkProperly()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync("Claims-2", "Commissions-1");

			var result = await countySelectorBase.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

			var claims = await countySelectorBase.GetClaimsAsync(result);

			Assert.AreEqual(11, claims.Count);

			var (key, value) = claims.ElementAt(0);
			Assert.AreEqual("selected_commissionHsaId", key);
			Assert.AreEqual(1, value.Count);
			Assert.AreEqual("selected_commissionHsaId", value.First().Type);

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
		public async Task Group_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("County", countySelectorBase.Group);
		}

		[TestMethod]
		public async Task SelectAsync_ShouldWorkProperly()
		{
			// First test
			var countySelectorBase = await this.CreateCountySelectorBaseAsync("Claims-1", "Commissions-1");

			var result = await countySelectorBase.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

			Assert.IsNotNull(result);
			Assert.IsFalse(result.Complete);
			Assert.AreEqual(1, result.Selectables.Count);
			Assert.AreEqual(4, result.Selectables[countySelectorBase.Group].Count);

			// Second test
			var value = result.Selectables[countySelectorBase.Group].First().Value;
			var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ countySelectorBase.Group, value }
			};

			result = await countySelectorBase.SelectAsync(selections);

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Complete);
			Assert.AreEqual(1, result.Selectables.Count);
			Assert.AreEqual(4, result.Selectables[countySelectorBase.Group].Count);
			Assert.IsTrue(result.Selectables[countySelectorBase.Group].First().Selected);

			// Third test
			countySelectorBase = await this.CreateCountySelectorBaseAsync("Claims-2", "Commissions-1");

			result = await countySelectorBase.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Complete);
			Assert.AreEqual(1, result.Selectables.Count);
			Assert.AreEqual(4, result.Selectables[countySelectorBase.Group].Count);
			Assert.IsTrue(result.Selectables[countySelectorBase.Group].Last().Selected);
		}

		[TestMethod]
		public async Task SelectedClaimTypePrefix_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("selected_", countySelectorBase.SelectedClaimTypePrefix);
		}

		#endregion
	}
}