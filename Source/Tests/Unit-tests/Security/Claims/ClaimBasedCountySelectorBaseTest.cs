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
	public class ClaimBasedCountySelectorBaseTest : ClaimsSelectorTestBase
	{
		#region Methods

		protected internal virtual async Task<ClaimBasedCountySelectorBase> CreateClaimBasedCountySelectorBaseAsync(string commissionsFileName, ILoggerFactory loggerFactory = null)
		{
			var selections = await this.CreateSelectionsAsync(commissionsFileName);

			return await this.CreateClaimBasedCountySelectorBaseAsync(loggerFactory, selections);
		}

		protected internal virtual async Task<ClaimBasedCountySelectorBase> CreateClaimBasedCountySelectorBaseAsync(ILoggerFactory loggerFactory = null, IList<Selection> selections = null)
		{
			var claimBasedCountySelectorBaseMock = new Mock<ClaimBasedCountySelectorBase>(loggerFactory ?? Mock.Of<ILoggerFactory>()) { CallBase = true };

			claimBasedCountySelectorBaseMock.Setup(claimBasedCountySelectorBase => claimBasedCountySelectorBase.GetSelectionsAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(selections ?? new List<Selection>()));

			return await Task.FromResult(claimBasedCountySelectorBaseMock.Object);
		}

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

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndNoSelections_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-Empty", loggerFactory);
				claimBasedCountySelectorBase.SelectionRequired = false;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.IsNotNull(claims);
				Assert.IsFalse(claims.Any());
			}
		}

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndOnlyOneSelectionAndNotSelected_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-Only-One", loggerFactory);
				claimBasedCountySelectorBase.SelectionRequired = false;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);

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
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-Empty", loggerFactory);
				claimBasedCountySelectorBase.Key = key;
				claimBasedCountySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);
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
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-1", loggerFactory);
				claimBasedCountySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);
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
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-Only-One", loggerFactory);
				claimBasedCountySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);
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
			var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync();

			try
			{
				await claimBasedCountySelectorBase.GetClaimsAsync(Mock.Of<ClaimsPrincipal>(), new ClaimsSelectionResult(Mock.Of<IClaimsSelector>()));
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
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-1", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-2");

				var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				var claims = await claimBasedCountySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.AreEqual(18, claims.Count);

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
				Assert.AreEqual("selected_givenName", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(6);
				Assert.AreEqual("selected_healthCareProviderHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderHsaId", value.First().Type);

				(key, value) = claims.ElementAt(7);
				Assert.AreEqual("selected_healthCareProviderName", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderName", value.First().Type);

				(key, value) = claims.ElementAt(8);
				Assert.AreEqual("selected_healthCareProviderOrgNo", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareProviderOrgNo", value.First().Type);

				(key, value) = claims.ElementAt(9);
				Assert.AreEqual("selected_healthCareUnitHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitHsaId", value.First().Type);

				(key, value) = claims.ElementAt(10);
				Assert.AreEqual("selected_healthCareUnitName", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitName", value.First().Type);

				(key, value) = claims.ElementAt(11);
				Assert.AreEqual("selected_healthCareUnitStartDate", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_healthCareUnitStartDate", value.First().Type);

				(key, value) = claims.ElementAt(12);
				Assert.AreEqual("selected_mail", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(13);
				Assert.AreEqual("selected_paTitleCode", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(14);
				Assert.AreEqual("selected_personalIdentityNumber", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(15);
				Assert.AreEqual("selected_personalPrescriptionCode", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(16);
				Assert.AreEqual("selected_surname", key);
				Assert.AreEqual(0, value.Count);

				(key, value) = claims.ElementAt(17);
				Assert.AreEqual("selected_systemRole", key);
				Assert.AreEqual(0, value.Count);
			}
		}

		[TestMethod]
		public async Task SelectAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var claimBasedCountySelectorBase = await this.CreateClaimBasedCountySelectorBaseAsync("Commissions-1", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				// First test
				var result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[claimBasedCountySelectorBase.Key].Count);

				// Second test
				var value = result.Selectables[claimBasedCountySelectorBase.Key].First().Value;
				var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{ claimBasedCountySelectorBase.Key, value }
				};

				result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, selections);

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[claimBasedCountySelectorBase.Key].Count);
				Assert.IsTrue(result.Selectables[claimBasedCountySelectorBase.Key].First().Selected);

				// Third test
				claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-2");

				result = await claimBasedCountySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(4, result.Selectables[claimBasedCountySelectorBase.Key].Count);
				Assert.IsTrue(result.Selectables[claimBasedCountySelectorBase.Key].Last().Selected);
			}
		}

		#endregion
	}
}