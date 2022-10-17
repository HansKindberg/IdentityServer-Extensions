using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Security.Claims.Configuration
{
	[TestClass]
	public class ClaimsSelectionOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task AutomaticSelectionClaimType_ShouldReturnADefaultValue()
		{
			await Task.CompletedTask;
			Assert.AreEqual("automatic_claims_selection", new ClaimsSelectionOptions().AutomaticSelectionClaimType);
		}

		[TestMethod]
		public async Task AutomaticSelectionEnabled_ShouldReturnTrueByDefault()
		{
			await Task.CompletedTask;
			Assert.IsTrue(new ClaimsSelectionOptions().AutomaticSelectionEnabled);
		}

		[TestMethod]
		public async Task DefaultReturnUrl_ShouldReturnADefaultValue()
		{
			await Task.CompletedTask;
			Assert.AreEqual("/Account", new ClaimsSelectionOptions().DefaultReturnUrl);
		}

		[TestMethod]
		public async Task Path_ShouldReturnADefaultValue()
		{
			await Task.CompletedTask;
			Assert.AreEqual($"/{nameof(Feature.ClaimsSelection)}", new ClaimsSelectionOptions().Path);
		}

		[TestMethod]
		public async Task Selectors_ShouldReturnAnEmptyDictionaryByDefault()
		{
			await Task.CompletedTask;
			Assert.IsFalse(new ClaimsSelectionOptions().Selectors.Any());
		}

		#endregion
	}
}