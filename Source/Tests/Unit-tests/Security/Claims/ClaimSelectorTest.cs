using System;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class ClaimsSelectorTest
	{
		#region Methods

		protected internal virtual async Task<ClaimsSelector> CreateClaimsSelectorAsync()
		{
			var claimsSelectorMock = new Mock<ClaimsSelector>(Mock.Of<ILoggerFactory>()) { CallBase = true };

			return await Task.FromResult(claimsSelectorMock.Object);
		}

		[TestMethod]
		public async Task Key_ShouldReturnAGuidAsStringWithoutHyphensByDefault()
		{
			var firstClaimsSelector = await this.CreateClaimsSelectorAsync();

			var firstCallKey = firstClaimsSelector.Key;
			var secondCallKey = firstClaimsSelector.Key;

			Assert.AreEqual(firstCallKey, secondCallKey);

			var guid = new Guid(firstCallKey);

			Assert.IsTrue(guid != Guid.Empty);
			Assert.IsFalse(firstCallKey.Contains('-', StringComparison.Ordinal));

			var secondClaimsSelector = await this.CreateClaimsSelectorAsync();

			Assert.AreNotEqual(firstClaimsSelector.Key, secondClaimsSelector.Key);
		}

		#endregion
	}
}