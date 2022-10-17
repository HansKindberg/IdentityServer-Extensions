using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using TestHelpers.Mocks.Logging;
using TestHelpers.Security.Claims;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class CountyIdentitySelectorBaseTest : ClaimsSelectorTestBase
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

		protected internal virtual async Task<CountyIdentitySelectorBase> CreateCountyIdentitySelectorBaseAsync(string claimsFileName, ILoggerFactory loggerFactory, string employeeHsaIdClaimType = "hsa_identity")
		{
			var claimsIdentity = await this.CreateClaimsPrincipalAsync(claimsFileName);

			var identities = claimsIdentity.FindAll(employeeHsaIdClaimType).Select(claim => claim.Value).ToList();

			return await this.CreateCountyIdentitySelectorBaseAsync(identities, loggerFactory);
		}

		protected internal virtual async Task<CountyIdentitySelectorBase> CreateCountyIdentitySelectorBaseAsync(IList<string> identities = null, ILoggerFactory loggerFactory = null)
		{
			var countyIdentitySelectorBaseMock = new Mock<CountyIdentitySelectorBase>(loggerFactory ?? Mock.Of<ILoggerFactory>()) { CallBase = true };

			countyIdentitySelectorBaseMock.Setup(countyIdentitySelectorBase => countyIdentitySelectorBase.GetIdentitiesAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(identities ?? new List<string>()));

			return await Task.FromResult(countyIdentitySelectorBaseMock.Object);
		}

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndNoIdentities_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-0", loggerFactory);
				countyIdentitySelectorBase.SelectionRequired = false;

				var identities = await countyIdentitySelectorBase.GetIdentitiesAsync(Mock.Of<ClaimsPrincipal>());
				Assert.IsFalse(identities.Any());

				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-0");

				var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await countyIdentitySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.IsNotNull(claims);
				Assert.IsFalse(claims.Any());
			}
		}

		[TestMethod]
		public async Task GetClaimsAsync_IfNotSelectionRequiredAndOnlyOneIdentityAndNotSelected_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-1", loggerFactory);
				countyIdentitySelectorBase.SelectionRequired = false;

				var identities = await countyIdentitySelectorBase.GetIdentitiesAsync(Mock.Of<ClaimsPrincipal>());
				Assert.AreEqual(1, identities.Count);

				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-0");

				var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				var claims = await countyIdentitySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.IsNotNull(claims);
				Assert.IsFalse(claims.Any());
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task GetClaimsAsync_IfSelectionRequiredAndNoIdentities_ShouldThrowAnInvalidOperationException()
		{
			const string key = "Test";

			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-0", loggerFactory);
				countyIdentitySelectorBase.Key = key;
				countyIdentitySelectorBase.SelectionRequired = true;

				try
				{
					var result = await countyIdentitySelectorBase.SelectAsync(Mock.Of<ClaimsPrincipal>(), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countyIdentitySelectorBase.GetClaimsAsync(Mock.Of<ClaimsPrincipal>(), result);
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
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-3", loggerFactory);
				countyIdentitySelectorBase.SelectionRequired = true;
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-3");

				try
				{
					var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countyIdentitySelectorBase.GetClaimsAsync(claimsPrincipal, result);
				}
				catch(InvalidOperationException invalidOperationException)
				{
					if(invalidOperationException.Message == $"Selection required but there is no selected selectable of type \"{typeof(CountyIdentitySelectableClaim)}\".")
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task GetClaimsAsync_IfSelectionRequiredAndOnlyOneIdentityAndNotSelected_ShouldThrowAnInvalidOperationException()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-1", loggerFactory);
				countyIdentitySelectorBase.SelectionRequired = true;

				var identities = await countyIdentitySelectorBase.GetIdentitiesAsync(Mock.Of<ClaimsPrincipal>());
				Assert.AreEqual(1, identities.Count);

				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-1");

				try
				{
					var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
					await countyIdentitySelectorBase.GetClaimsAsync(claimsPrincipal, result);
				}
				catch(InvalidOperationException invalidOperationException)
				{
					if(invalidOperationException.Message == $"Selection required but there is no selected selectable of type \"{typeof(CountyIdentitySelectableClaim)}\".")
						throw;
				}
			}
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task GetClaimsAsync_IfTheSelectorPropertyOfTheSelectionResultParameterIsNotTheCountySelectorItSelf_ShouldThrowAnArgumentException()
		{
			var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync();

			try
			{
				await countyIdentitySelectorBase.GetClaimsAsync(Mock.Of<ClaimsPrincipal>(), new ClaimsSelectionResult(Mock.Of<IClaimsSelector>()));
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
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-3", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-3-Selected");

				var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				var claims = await countyIdentitySelectorBase.GetClaimsAsync(claimsPrincipal, result);

				Assert.AreEqual(1, claims.Count);

				var (key, value) = claims.ElementAt(0);
				Assert.AreEqual("selected_employeeHsaId", key);
				Assert.AreEqual(1, value.Count);
				Assert.AreEqual("selected_employeeHsaId", value.First().Type);
				Assert.AreEqual("TEST123456-e002", value.First().Value);
			}
		}

		[TestMethod]
		public async Task SelectAsync_ShouldWorkProperly()
		{
			using(var loggerFactory = LoggerFactoryMock.Create())
			{
				var countyIdentitySelectorBase = await this.CreateCountyIdentitySelectorBaseAsync("Claims-3", loggerFactory);
				var claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-3");

				// First test
				var result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(3, result.Selectables[countyIdentitySelectorBase.Key].Count);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(0).Selected);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(1).Selected);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(2).Selected);

				// Second test
				var value = result.Selectables[countyIdentitySelectorBase.Key].First().Value;
				var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{ countyIdentitySelectorBase.Key, value }
				};

				result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, selections);

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(3, result.Selectables[countyIdentitySelectorBase.Key].Count);
				Assert.IsTrue(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(0).Selected);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(1).Selected);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(2).Selected);

				// Third test
				claimsPrincipal = await this.CreateClaimsPrincipalAsync("Claims-3-Selected");

				result = await countyIdentitySelectorBase.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Selectables.Count);
				Assert.AreEqual(3, result.Selectables[countyIdentitySelectorBase.Key].Count);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(0).Selected);
				Assert.IsTrue(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(1).Selected);
				Assert.IsFalse(result.Selectables[countyIdentitySelectorBase.Key].ElementAt(2).Selected);
			}
		}

		#endregion
	}
}