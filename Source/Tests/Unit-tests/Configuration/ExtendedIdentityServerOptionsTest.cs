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
		public async Task Account_ShouldHaveADefaultValue()
		{
			await Task.CompletedTask;

			var account = new ExtendedIdentityServerOptions().Account;

			Assert.IsNotNull(account);
		}

		#endregion
	}
}