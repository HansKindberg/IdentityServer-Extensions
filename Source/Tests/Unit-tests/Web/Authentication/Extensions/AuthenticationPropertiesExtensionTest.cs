using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Web.Authentication.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Web.Authentication.Extensions
{
	[TestClass]
	public class AuthenticationPropertiesExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task GetClaimsSelectionClaimTypesAsync_MayReturnAnEmptySet()
		{
			var authenticationProperties = new AuthenticationProperties();

			await authenticationProperties.SetClaimsSelectionClaimTypesAsync(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

			var claimTypes = await authenticationProperties.GetClaimsSelectionClaimTypesAsync();

			Assert.IsNotNull(claimTypes);
			Assert.IsFalse(claimTypes.Any());
		}

		[TestMethod]
		public async Task GetClaimsSelectionClaimTypesAsync_MayReturnNull()
		{
			var authenticationProperties = new AuthenticationProperties();

			Assert.IsNull(await authenticationProperties.GetClaimsSelectionClaimTypesAsync());

			await authenticationProperties.SetClaimsSelectionClaimTypesAsync(null);

			Assert.IsNull(await authenticationProperties.GetClaimsSelectionClaimTypesAsync());
		}

		[TestMethod]
		public async Task GetClaimsSelectionClaimTypesAsync_ShouldReturnASortedSet()
		{
			ISet<string> claimTypes = new HashSet<string>()
			{
				"claim-4",
				"claim-3",
				"claim-1",
				"claim-2",
				"CLAIM-4",
				"claim-4"
			};

			var authenticationProperties = new AuthenticationProperties();

			await authenticationProperties.SetClaimsSelectionClaimTypesAsync(claimTypes);

			claimTypes = await authenticationProperties.GetClaimsSelectionClaimTypesAsync();

			Assert.IsNotNull(claimTypes);
			Assert.AreEqual(4, claimTypes.Count);
			Assert.AreEqual("claim-1", claimTypes.ElementAt(0));
			Assert.AreEqual("claim-2", claimTypes.ElementAt(1));
			Assert.AreEqual("claim-3", claimTypes.ElementAt(2));
			Assert.AreEqual("claim-4", claimTypes.ElementAt(3));
		}

		#endregion
	}
}