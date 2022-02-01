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
using TestHelpers.Mocks.Logging;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountyCommissionClaimsSelectorTest : CountyClaimsSelectorTestBase
	{
		#region Methods

		[TestMethod]
		public async Task AllCommissionsClaimType_ShouldReturnAllCommissionsByDefault()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();
			Assert.AreEqual("allCommissions", countyCommissionClaimsSelector.AllCommissionsClaimType);
		}

		protected internal virtual async Task<CountyCommissionClaimsSelector> CreateCountyCommissionClaimsSelectorAsync()
		{
			return await this.CreateCountyCommissionClaimsSelectorAsync(null);
		}

		protected internal virtual async Task<CountyCommissionClaimsSelector> CreateCountyCommissionClaimsSelectorAsync(IEnumerable<Claim> claims)
		{
			return await this.CreateCountyCommissionClaimsSelectorAsync(Mock.Of<ILoggerFactory>(), claims);
		}

		protected internal virtual async Task<CountyCommissionClaimsSelector> CreateCountyCommissionClaimsSelectorAsync(ILoggerFactory loggerFactory, IEnumerable<Claim> claims = null, bool isAuthenticated = true)
		{
			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims, isAuthenticated);

			var httpContextMock = new Mock<HttpContext>();
			httpContextMock.Setup(httpContext => httpContext.User).Returns(claimsPrincipal);
			var httpContext = httpContextMock.Object;

			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);
			var httpContextAccessor = httpContextAccessorMock.Object;

			return await this.CreateCountyCommissionClaimsSelectorAsync(httpContextAccessor, loggerFactory);
		}

		protected internal virtual async Task<CountyCommissionClaimsSelector> CreateCountyCommissionClaimsSelectorAsync(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
		{
			return await Task.FromResult(new CountyCommissionClaimsSelector(httpContextAccessor, loggerFactory));
		}

		[TestMethod]
		public async Task GetAllCommissionsAsync_IfTheAllCommissionsClaimValueIsAnEmptyString_ShouldReturnAnEmptyList()
		{
			var claims = new List<Claim>
			{
				new("allCommissions", string.Empty)
			};

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var allCommissions = await countyCommissionClaimsSelector.GetAllCommissionsAsync(claimsPrincipal);

			Assert.IsNotNull(allCommissions);
			Assert.IsFalse(allCommissions.Any());
		}

		[TestMethod]
		public async Task GetAllCommissionsAsync_IfTheAllCommissionsClaimValueIsInvalid_ShouldReturnAnEmptyListAndLogTheException()
		{
			var claims = new List<Claim>
			{
				new("allCommissions", "Invalid")
			};

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);

			using(var loggerFactory = Global.CreateLoggerFactoryMock())
			{
				loggerFactory.EnabledMode = LogLevelEnabledMode.Enabled;

				var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(loggerFactory);

				var allCommissions = await countyCommissionClaimsSelector.GetAllCommissionsAsync(claimsPrincipal);

				Assert.IsNotNull(allCommissions);
				Assert.IsFalse(allCommissions.Any());

				Assert.AreEqual(1, loggerFactory.Logs.Count());
				var log = loggerFactory.Logs.First();
				Assert.AreEqual("Could not deserialize the following json to a commission-list: Invalid", log.Message);
				Assert.IsTrue(log.Exception is JsonReaderException);
			}
		}

		[TestMethod]
		public async Task GetAllCommissionsAsync_IfTheAllCommissionsClaimValueIsWhitespaces_ShouldReturnAnEmptyList()
		{
			var claims = new List<Claim>
			{
				new("allCommissions", "   ")
			};

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var allCommissions = await countyCommissionClaimsSelector.GetAllCommissionsAsync(claimsPrincipal);

			Assert.IsNotNull(allCommissions);
			Assert.IsFalse(allCommissions.Any());
		}

		[TestMethod]
		public async Task GetAllCommissionsAsync_IfTheClaimsPrincipalDoesNotHaveAnAllCommissionsClaim_ShouldReturnAnEmptyList()
		{
			var claimsPrincipal = await this.CreateClaimsPrincipalAsync();
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var allCommissions = await countyCommissionClaimsSelector.GetAllCommissionsAsync(claimsPrincipal);

			Assert.IsNotNull(allCommissions);
			Assert.IsFalse(allCommissions.Any());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetAllCommissionsAsync_IfTheClaimsPrincipalParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			try
			{
				_ = await countyCommissionClaimsSelector.GetAllCommissionsAsync(null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(string.Equals(argumentNullException.ParamName, "claimsPrincipal", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		public async Task GetClaimsAsync_Test()
		{
			var commissions = await this.CreateCommissionsAsync(5);

			var employeeHsaId = commissions[0].EmployeeHsaId;

			commissions[2].EmployeeHsaId = commissions[4].EmployeeHsaId = employeeHsaId;

			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
				new(nameof(employeeHsaId), employeeHsaId)
			};

			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(claims);

			var commissionHsaId = commissions[2].CommissionHsaId;
			var claimsSelectionResult = await countyCommissionClaimsSelector.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { countyCommissionClaimsSelector.Group, $"{employeeHsaId}:{commissionHsaId}" } });

			var claimsResult = await countyCommissionClaimsSelector.GetClaimsAsync(claimsSelectionResult);

			Assert.AreEqual(9, claimsResult.Count);

			var commissionHsaIdClaimType = nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant();
			var first = claimsResult.ElementAt(0);
			Assert.AreEqual(commissionHsaIdClaimType, first.Key);
			Assert.AreEqual(commissionHsaIdClaimType, first.Value[0].Type);
			Assert.AreEqual(commissionHsaId, first.Value[0].Value);
		}

		[TestMethod]
		public async Task GetCommissionMapAsync_IfTheClaimsParameterHasNoClaims_ShouldReturnAnEmptyDictionary()
		{
			var claimsPrincipal = await this.CreateClaimsPrincipalAsync();
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var commissionMap = await countyCommissionClaimsSelector.GetCommissionMapAsync(claimsPrincipal);

			Assert.IsNotNull(commissionMap);
			Assert.IsFalse(commissionMap.Any());
		}

		[TestMethod]
		public async Task GetCommissionMapAsync_IfTheClaimsParameterHasNoEmployeeHsaIdClaim_ShouldReturnAnEmptyDictionary()
		{
			var commissions = await this.CreateCommissionsAsync(5);
			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions))
			};

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var commissionMap = await countyCommissionClaimsSelector.GetCommissionMapAsync(claimsPrincipal);

			Assert.IsNotNull(commissionMap);
			Assert.IsFalse(commissionMap.Any());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task GetCommissionMapAsync_IfTheClaimsPrincipalParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			try
			{
				_ = await countyCommissionClaimsSelector.GetCommissionMapAsync(null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(string.Equals(argumentNullException.ParamName, "claimsPrincipal", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		public async Task GetCommissionMapAsync_Test()
		{
			var commissions = await this.CreateCommissionsAsync(5);

			var employeeHsaId = commissions[0].EmployeeHsaId;

			commissions[2].EmployeeHsaId = commissions[4].EmployeeHsaId = employeeHsaId;

			var claims = new List<Claim>
			{
				new("allCommissions", JsonConvert.SerializeObject(commissions)),
				new(nameof(employeeHsaId), employeeHsaId)
			};

			var claimsPrincipal = await this.CreateClaimsPrincipalAsync(claims);
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();

			var commissionMap = await countyCommissionClaimsSelector.GetCommissionMapAsync(claimsPrincipal);

			Assert.IsNotNull(commissionMap);
			Assert.AreEqual(3, commissionMap.Count);
		}

		[TestMethod]
		public async Task Group_ShouldReturnCommissionByDefault()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync();
			Assert.AreEqual("Commission", countyCommissionClaimsSelector.Group);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task SelectAsync_IfTheSelectionsParameterIsNotNullAndIfTheHttpContextIsNull_ShouldThrowAnInvalidOperationException()
		{
			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns((HttpContext)null);
			var httpContextAccessor = httpContextAccessorMock.Object;

			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(httpContextAccessor, Mock.Of<ILoggerFactory>());

			try
			{
				_ = await countyCommissionClaimsSelector.SelectAsync(new Dictionary<string, string>());
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(string.Equals(invalidOperationException.Message, "The http-context is null.", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task SelectAsync_IfTheSelectionsParameterIsNotNullAndIfTheHttpContextUserIsNotAuthenticated_ShouldThrowAnInvalidOperationException()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(Mock.Of<ILoggerFactory>(), isAuthenticated: false);

			try
			{
				_ = await countyCommissionClaimsSelector.SelectAsync(new Dictionary<string, string>());
			}
			catch(InvalidOperationException invalidOperationException)
			{
				if(string.Equals(invalidOperationException.Message, "The http-context-user is not authenticated.", StringComparison.Ordinal))
					throw;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task SelectAsync_IfTheSelectionsParameterIsNull_ShouldThrowAnArgumentNullException()
		{
			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(null);

			try
			{
				_ = await countyCommissionClaimsSelector.SelectAsync(null);
			}
			catch(ArgumentNullException argumentNullException)
			{
				if(string.Equals(argumentNullException.ParamName, "selections", StringComparison.Ordinal))
					throw;
			}
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
				new(nameof(employeeHsaId), employeeHsaId)
			};

			var countyCommissionClaimsSelector = await this.CreateCountyCommissionClaimsSelectorAsync(claims);

			var claimsSelectionResult = await countyCommissionClaimsSelector.SelectAsync(new Dictionary<string, string>());
			Assert.IsNotNull(claimsSelectionResult);
			Assert.IsFalse(claimsSelectionResult.Complete);
			Assert.AreEqual(1, claimsSelectionResult.Selectables.Count);
			Assert.AreEqual(3, claimsSelectionResult.Selectables[countyCommissionClaimsSelector.Group].Count);
			Assert.IsFalse(claimsSelectionResult.Selectables[countyCommissionClaimsSelector.Group].Any(selectable => selectable.Selected));
			Assert.AreEqual(countyCommissionClaimsSelector, claimsSelectionResult.Selector);

			claimsSelectionResult = await countyCommissionClaimsSelector.SelectAsync(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { countyCommissionClaimsSelector.Group, $"{employeeHsaId}:{commissions[2].CommissionHsaId}" } });
			Assert.IsNotNull(claimsSelectionResult);
			Assert.IsTrue(claimsSelectionResult.Complete);
			Assert.AreEqual(1, claimsSelectionResult.Selectables.Count);
			Assert.AreEqual(3, claimsSelectionResult.Selectables[countyCommissionClaimsSelector.Group].Count);
			Assert.IsTrue(claimsSelectionResult.Selectables[countyCommissionClaimsSelector.Group][1].Selected);
			Assert.AreEqual(countyCommissionClaimsSelector, claimsSelectionResult.Selector);
		}

		#endregion
	}
}