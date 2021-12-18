using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Configuration
{
	[TestClass]
	public class ExtendedIdentityServerOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task FormsAuthentication_ShouldHaveADefaultValue()
		{
			await Task.CompletedTask;

			var formsAuthentication = new ExtendedIdentityServerOptions().FormsAuthentication;

			Assert.IsNotNull(formsAuthentication);
		}

		[TestMethod]
		public async Task SignOut_ShouldHaveADefaultValue()
		{
			await Task.CompletedTask;

			var signOut = new ExtendedIdentityServerOptions().SignOut;

			Assert.IsNotNull(signOut);
		}

		#endregion
	}
}