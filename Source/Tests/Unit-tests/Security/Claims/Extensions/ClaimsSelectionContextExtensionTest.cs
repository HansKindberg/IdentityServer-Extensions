using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using HansKindberg.IdentityServer.Security.Claims.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RegionOrebroLan.Security.Claims;
using TestHelpers.Security.Claims;
using UnitTests.Mocks.Security.Claims;

namespace UnitTests.Security.Claims.Extensions
{
	[TestClass]
	public class ClaimsSelectionContextExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test01()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.IsFalse(await claimsSelectionContext.AnySelectableClaimsAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test02()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(1));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.IsTrue(await claimsSelectionContext.AnySelectableClaimsAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test03()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(5));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(1));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(4));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.IsTrue(await claimsSelectionContext.AnySelectableClaimsAsync(claimsPrincipal));
		}

		protected internal virtual async Task<ClaimsSelectionContext> CreateClaimsSelectionContextAsync(IOptionsMonitor<ClaimsSelectionOptions> claimsSelectionOptionsMonitor = null)
		{
			var claimsSelectionContext = new ClaimsSelectionContext(claimsSelectionOptionsMonitor ?? await this.CreateClaimsSelectionOptionsMonitorAsync(new ClaimsSelectionOptions()));

			return await Task.FromResult(claimsSelectionContext);
		}

		protected internal virtual async Task<IOptionsMonitor<ClaimsSelectionOptions>> CreateClaimsSelectionOptionsMonitorAsync(ClaimsSelectionOptions claimsSelectionOptions)
		{
			var claimsSelectionOptionsMonitorMock = new Mock<IOptionsMonitor<ClaimsSelectionOptions>>();

			claimsSelectionOptionsMonitorMock.Setup(claimsSelectionOptionsMonitor => claimsSelectionOptionsMonitor.CurrentValue).Returns(claimsSelectionOptions);

			return await Task.FromResult(claimsSelectionOptionsMonitorMock.Object);
		}

		protected internal virtual async Task<ClaimsSelectorMock> CreateClaimsSelectorMockAsync(byte numberOfSelectableClaims, byte numberOfSelectables = 1)
		{
			var claimsSelectionResult = new ClaimsSelectionResultMock();

			for(var i = 0; i < numberOfSelectables; i++)
			{
				var selectableClaims = new List<ISelectableClaim>();

				for(var j = 0; j < numberOfSelectableClaims; j++)
				{
					selectableClaims.Add(Mock.Of<ISelectableClaim>());
				}

				claimsSelectionResult.Selectables.Add(i.ToString(CultureInfo.InvariantCulture), selectableClaims);
			}

			var claimsSelectorMock = new ClaimsSelectorMock
			{
				ClaimsSelectionResult = claimsSelectionResult
			};

			return await Task.FromResult(claimsSelectorMock);
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test01()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.AreEqual(0, await claimsSelectionContext.NumberOfSelectableClaimsAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test02()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(1));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(0));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.AreEqual(1, await claimsSelectionContext.NumberOfSelectableClaimsAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test03()
		{
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(new ClaimBuilderCollection());
			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync();

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(5));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(1));
			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(4));

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);

			Assert.AreEqual(10, await claimsSelectionContext.NumberOfSelectableClaimsAsync(claimsPrincipal));
		}

		#endregion
	}
}