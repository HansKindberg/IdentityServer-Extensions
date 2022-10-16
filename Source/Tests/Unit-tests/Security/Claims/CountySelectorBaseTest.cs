using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using TestHelpers.Mocks.Logging;
using TestHelpers.Security.Claims;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountySelectorBaseTest : ClaimsSelectorTestBase
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

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync(string commissionsFileName, ILoggerFactory loggerFactory = null)
		{
			var selections = await this.CreateSelectionsAsync(commissionsFileName);

			return await this.CreateCountySelectorBaseAsync(loggerFactory, selections);
		}

		protected internal virtual async Task<CountySelectorBase> CreateCountySelectorBaseAsync(ILoggerFactory loggerFactory = null, IList<Selection> selections = null)
		{
			var countySelectorBaseMock = new Mock<CountySelectorBase>(loggerFactory ?? Mock.Of<ILoggerFactory>()) { CallBase = true };

			countySelectorBaseMock.Setup(countySelectorBase => countySelectorBase.GetSelectionsAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(selections ?? new List<Selection>()));

			return await Task.FromResult(countySelectorBaseMock.Object);
		}

		[TestMethod]
		public async Task EmployeeHsaIdClaimType_ShouldReturnADefaultValue()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			Assert.AreEqual("hsa_identity", countySelectorBase.EmployeeHsaIdClaimType);
		}

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndNoSelections_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-Empty", loggerFactory);
				countySelectorBase.SelectionRequired = false;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.IsNotNull(claims);
				Assert.IsFalse(claims.Any());
			}
		}

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndOnlyOneSelectionAndNotSelected_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-Only-One", loggerFactory);
				countySelectorBase.SelectionRequired = false;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.IsNotNull(claims);
				Assert.IsFalse(claims.Any());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task GetClaimsAsync_IfSelectionRequiredAndNoSelections_ShouldThrowAnInvalidOperationException()
		{
			const string key = "Test";

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-Empty", loggerFactory);
				countySelectorBase.Key = key;
				countySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);
				}
				catch(InvalidOperationException invalidOperationException)
				{
					if(invalidOperationException.Message == $"There is no selectable with key {key.ToStringRepresentation()}.")
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task GetClaimsAsync_IfSelectionRequiredAndNotSelected_ShouldThrowAnInvalidOperationException()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-1", loggerFactory);
				countySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);
				}
				catch(InvalidOperationException invalidOperationException)
				{
					if(invalidOperationException.Message == $"Selection required but there is no selected selectable of type \"{typeof(CountySelectableClaim)}\".")
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task GetClaimsAsync_IfSelectionRequiredAndOnlyOneSelectionAndNotSelected_ShouldThrowAnInvalidOperationException()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-Only-One", loggerFactory);
				countySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);
				}
				catch(InvalidOperationException invalidOperationException)
				{
					if(invalidOperationException.Message == $"Selection required but there is no selected selectable of type \"{typeof(CountySelectableClaim)}\".")
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task GetClaimsAsync_IfTheSelectorPropertyOfTheSelectionResultParameterIsNotTheCountySelectorItSelf_ShouldThrowAnArgumentException()
		{
			var countySelectorBase = await this.CreateCountySelectorBaseAsync();

			try
			{
				await countySelectorBase.GetClaimsAsync(Mock.Of<ClaimsPrincipal>(), new ClaimsSelectionResult(Mock.Of<IClaimsSelector>()));
			}
			catch(ArgumentException argumentException)
			{
				if(argumentException.Message.StartsWith("The selector-property of the selection-result is not this instance.", StringComparison.Ordinal) && argumentException.ParamName == "selectionResult")
					throw;
			}
		}

		[TestMethod]
		public async Task GetClaimsAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-1", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-2");

				var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				var claims = await countySelectorBase.GetClaimsAsync(claimsPrincipal, result);

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
		public async Task SelectAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countySelectorBase = await this.CreateCountySelectorBaseAsync("Commissions-1", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				// First test
				var result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[countySelectorBase.Key].Count);

				// Second test
				var value = result.Selectables[countySelectorBase.Key].First().Value;
				var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{ countySelectorBase.Key, value }
				};

				result = await countySelectorBase.SelectAsync(claimsPrincipal, selections);

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[countySelectorBase.Key].Count);
				Assert.IsTrue(result.Selectables[countySelectorBase.Key].First().Selected);

				// Third test
				claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-2");

				result = await countySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[countySelectorBase.Key].Count);
				Assert.IsTrue(result.Selectables[countySelectorBase.Key].Last().Selected);
			}
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