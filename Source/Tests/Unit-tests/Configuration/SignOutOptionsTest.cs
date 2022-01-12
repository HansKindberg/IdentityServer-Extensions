using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Configuration
{
	[TestClass]
	public class SignOutOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task AutomaticRedirectAfterSignOut_ShouldReturnTrueByDefault()
		{
			await Task.CompletedTask;

			Assert.IsTrue(new SignOutOptions().AutomaticRedirectAfterSignOut);
		}

		[TestMethod]
		public async Task ConfirmSignOut_ShouldReturnTrueByDefault()
		{
			await Task.CompletedTask;

			Assert.IsTrue(new SignOutOptions().ConfirmSignOut);
		}

		[TestMethod]
		public async Task Mode_ShouldReturnClientInitiatedAndIdpInitiatedByDefault()
		{
			await Task.CompletedTask;

			var mode = new SignOutOptions().Mode;

			Assert.IsTrue(mode.HasFlag(SingleSignOutMode.ClientInitiated));
			Assert.IsTrue(mode.HasFlag(SingleSignOutMode.IdpInitiated));
		}

		[TestMethod]
		public async Task SecondsBeforeRedirectAfterSignOut_ShouldReturnFourByDefault()
		{
			await Task.CompletedTask;

			Assert.AreEqual(4, new SignOutOptions().SecondsBeforeRedirectAfterSignOut);
		}

		#endregion
	}
}