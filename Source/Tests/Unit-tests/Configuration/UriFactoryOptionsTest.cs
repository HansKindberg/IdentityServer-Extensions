using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Configuration
{
	[TestClass]
	public class UriFactoryOptionsTest
	{
		#region Methods

		[TestMethod]
		public async Task TrailingPathSlash_ShouldReturnTrueByDefault()
		{
			await Task.CompletedTask;

			Assert.IsTrue(new UriFactoryOptions().TrailingPathSlash);
		}

		[TestMethod]
		public async Task UiLocalesInReturnUrl_ShouldReturnTrueByDefault()
		{
			await Task.CompletedTask;

			Assert.IsTrue(new UriFactoryOptions().UiLocalesInReturnUrl);
		}

		#endregion
	}
}