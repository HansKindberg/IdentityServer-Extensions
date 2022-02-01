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
using Newtonsoft.Json;
using RegionOrebroLan.Localization.Extensions;
using RegionOrebroLan.Web.Authentication.DirectoryServices;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountyClaimsSelectorTest : CountyClaimsSelectorTestBase
	{
		#region Methods

		protected internal virtual async Task<CountyClaimsSelector> CreateCountyClaimsSelectorAsync()
		{
			return await this.CreateCountyClaimsSelectorAsync(null);
		}

		protected internal virtual async Task<CountyClaimsSelector> CreateCountyClaimsSelectorAsync(IEnumerable<Claim> claims)
		{
			return await this.CreateCountyClaimsSelectorAsync(null, claims);
		}

		protected internal virtual async Task<CountyClaimsSelector> CreateCountyClaimsSelectorAsync(IDictionary<string, string> activeDirectoryAttributes, IEnumerable<Claim> claims)
		{
			return await this.CreateCountyClaimsSelectorAsync(Mock.Of<ILoggerFactory>(), activeDirectoryAttributes, claims);
		}

		protected internal virtual async Task<CountyClaimsSelector> CreateCountyClaimsSelectorAsync(ILoggerFactory loggerFactory, IDictionary<string, string> activeDirectoryAttributes = null, IEnumerable<Claim> claims = null, bool isAuthenticated = true)
		{
			activeDirectoryAttributes ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			var activeDirectoryMock = new Mock<IActiveDirectory>();
			activeDirectoryMock.Setup(activeDirectory => activeDirectory.GetAttributesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<IdentifierKind>())).Returns(Task.FromResult(activeDirectoryAttributes));
			var activeDirectory = activeDirectoryMock.Object;

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims, isAuthenticated);

			var httpContextMock = new Mock<HttpContext>();
			httpContextMock.Setup(httpContext => httpContext.User).Returns(claimsPrincipal);
			var httpContext = httpContextMock.Object;

			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);
			var httpContextAccessor = httpContextAccessorMock.Object;

			return await this.CreateCountyClaimsSelectorAsync(activeDirectory, httpContextAccessor, loggerFactory);
		}

		protected internal virtual async Task<CountyClaimsSelector> CreateCountyClaimsSelectorAsync(IActiveDirectory activeDirectory, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
		{
			return await Task.FromResult(new CountyClaimsSelector(activeDirectory, httpContextAccessor, loggerFactory));
		}

		[TestMethod]
		public async Task GetClaimsAsync_Test()
		{
			var commissions = await this.CreateCommissionsAsync(5);

			commissions[2].EmployeeHsaId = commissions[4].EmployeeHsaId = commissions[0].EmployeeHsaId;

			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
			};

			var employeeHsaIds = commissions.Select(commission => commission.EmployeeHsaId).ToHashSet(StringComparer.OrdinalIgnoreCase);
			employeeHsaIds.Add("EmployeeHsaId-10");

			claims.AddRange(employeeHsaIds.Select(employeeHsaId => new Claim("allEmployeeHsaIds", employeeHsaId)));

			var countyClaimsSelector = await this.CreateCountyClaimsSelectorAsync(claims);

			countyClaimsSelector.ActiveDirectoryMap.Add("Dummy", "Dummy");
			countyClaimsSelector.SamAccountNamePrefix = "EmployeeHsaId-";

			var commissionHsaId = commissions[2].CommissionHsaId;
			var employeeHsaId = commissions[2].EmployeeHsaId;
			var claimsSelectionResult = await countyClaimsSelector.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { countyClaimsSelector.Group, $"{employeeHsaId}:{commissionHsaId}" } });

			var claimsResult = await countyClaimsSelector.GetClaimsAsync(claimsSelectionResult);

			Assert.AreEqual(11, claimsResult.Count);

			var commissionHsaIdClaimType = nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant();
			var first = claimsResult.ElementAt(0);
			Assert.AreEqual(commissionHsaIdClaimType, first.Key);
			Assert.AreEqual(commissionHsaIdClaimType, first.Value[0].Type);
			Assert.AreEqual(commissionHsaId, first.Value[0].Value);
		}

		[TestMethod]
		public async Task GetCommissionMapAsync_Test()
		{
			var commissions = await this.CreateCommissionsAsync(5);

			commissions[2].EmployeeHsaId = commissions[4].EmployeeHsaId = commissions[0].EmployeeHsaId;

			// First test
			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
			};

			var employeeHsaIds = commissions.Select(commission => commission.EmployeeHsaId).ToHashSet(StringComparer.OrdinalIgnoreCase);
			employeeHsaIds.Add("EmployeeHsaId-10");

			claims.AddRange(employeeHsaIds.Select(employeeHsaId => new Claim("allEmployeeHsaIds", employeeHsaId)));

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			var countyClaimsSelector = await this.CreateCountyClaimsSelectorAsync();

			var commissionMap = await countyClaimsSelector.GetCommissionMapAsync(claimsPrincipal);

			Assert.IsNotNull(commissionMap);
			Assert.AreEqual(6, commissionMap.Count);

			// Second test
			claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
			};

			employeeHsaIds = new HashSet<string>(new[] { "EmployeeHsaId-2", "EmployeeHsaId-10" }, StringComparer.OrdinalIgnoreCase);

			claims.AddRange(employeeHsaIds.Select(employeeHsaId => new Claim("allEmployeeHsaIds", employeeHsaId)));

			claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			countyClaimsSelector = await this.CreateCountyClaimsSelectorAsync();

			commissionMap = await countyClaimsSelector.GetCommissionMapAsync(claimsPrincipal);

			Assert.IsNotNull(commissionMap);
			Assert.AreEqual(2, commissionMap.Count);
		}

		[TestMethod]
		public async Task SelectAsync_Test()
		{
			var commissions = await this.CreateCommissionsAsync(5);

			var employeeHsaId = commissions[0].EmployeeHsaId;

			commissions[2].EmployeeHsaId = commissions[4].EmployeeHsaId = employeeHsaId;

			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
			};

			var employeeHsaIds = commissions.Select(commission => commission.EmployeeHsaId).ToHashSet(StringComparer.OrdinalIgnoreCase);
			employeeHsaIds.Add("EmployeeHsaId-10");

			claims.AddRange(employeeHsaIds.Select(item => new Claim("allEmployeeHsaIds", item)));

			var countyClaimsSelector = await this.CreateCountyClaimsSelectorAsync(claims);

			var claimsSelectionResult = await countyClaimsSelector.SelectAsync(new Dictionary<string, string>());
			Assert.IsNotNull(claimsSelectionResult);
			Assert.IsFalse(claimsSelectionResult.Complete);
			Assert.AreEqual(1, claimsSelectionResult.Selectables.Count);
			Assert.AreEqual(6, claimsSelectionResult.Selectables[countyClaimsSelector.Group].Count);
			Assert.IsFalse(claimsSelectionResult.Selectables[countyClaimsSelector.Group].Any(selectable => selectable.Selected));
			Assert.AreEqual(countyClaimsSelector, claimsSelectionResult.Selector);

			claimsSelectionResult = await countyClaimsSelector.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { countyClaimsSelector.Group, $"{employeeHsaId}:{commissions[2].CommissionHsaId}" } });
			Assert.IsNotNull(claimsSelectionResult);
			Assert.IsTrue(claimsSelectionResult.Complete);
			Assert.AreEqual(1, claimsSelectionResult.Selectables.Count);
			Assert.AreEqual(6, claimsSelectionResult.Selectables[countyClaimsSelector.Group].Count);
			Assert.IsTrue(claimsSelectionResult.Selectables[countyClaimsSelector.Group][1].Selected);
			Assert.AreEqual(countyClaimsSelector, claimsSelectionResult.Selector);
		}

		#endregion
	}
}