using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
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

namespace UnitTests.Security.Claims
{
	[TestClass]
	public class ClaimsSelectionContextTest
	{
		#region Methods

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereAreNoSelectors_ShouldReturnTrue()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			Assert.IsFalse(claimsSelectionContext.Selectors.Any());

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereAreSelectorsWithSelectablesWithZeroOrOneSelectableClaim_And_SelectionIsNotRequired_ShouldReturnFalse()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claimsSelector = await this.CreateClaimsSelectorMockAsync(1, 4);
			claimsSelector.SelectionRequired = false;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			claimsSelector = await this.CreateClaimsSelectorMockAsync(1, 4);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			claimsSelector = await this.CreateClaimsSelectorMockAsync(0, 4);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);
			Assert.AreEqual(8, await claimsSelectionContext.NumberOfSelectableClaimsAsync(Mock.Of<ClaimsPrincipal>()));

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereAreSelectorsWithSelectablesWithZeroOrOneSelectableClaim_And_SelectionIsRequired_ShouldReturnTrue()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claimsSelector = await this.CreateClaimsSelectorMockAsync(1, 4);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			claimsSelector = await this.CreateClaimsSelectorMockAsync(1, 4);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			claimsSelector = await this.CreateClaimsSelectorMockAsync(0, 4);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			Assert.AreEqual(3, claimsSelectionContext.Selectors.Count);
			Assert.AreEqual(8, await claimsSelectionContext.NumberOfSelectableClaimsAsync(Mock.Of<ClaimsPrincipal>()));

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereIsASelectorWithASelectableWithOnlyOneSelectableClaim_And_SelectionIsNotRequired_ShouldReturnFalse()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claimsSelector = await this.CreateClaimsSelectorMockAsync(1);
			claimsSelector.SelectionRequired = false;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereIsASelectorWithASelectableWithOnlyOneSelectableClaim_And_SelectionIsRequired_ShouldReturnTrue()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claimsSelector = await this.CreateClaimsSelectorMockAsync(1);
			claimsSelector.SelectionRequired = true;
			claimsSelectionContext.Selectors.Add(claimsSelector);

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task AutomaticSelectionIsPossibleAsync_IfAutomaticSelectionIsEnabled_And_IfThereIsAtLeastOneSelectorWithASelectableWithMoreThanOneSelectableClaim_ShouldReturnFalse()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			claimsSelectionContext.Selectors.Add(await this.CreateClaimsSelectorMockAsync(2));

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.AutomaticSelectionIsPossibleAsync(claimsPrincipal));
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
		public async Task IsAutomaticallySelectedAsync_IfAutomaticSelectionIsEnabled_And_IfThereIsAClaimWithATrueValue_ShouldReturnTrue()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = "true"
				}
			};
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = "tRuE"
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = "TRUE"
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsTrue(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task IsAutomaticallySelectedAsync_IfAutomaticSelectionIsEnabled_And_IfThereIsAClaimWithoutATrueValue_ShouldReturnTrue()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = "false"
				}
			};
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = "1"
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = string.Empty
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = null
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task IsAutomaticallySelectedAsync_IfAutomaticSelectionIsEnabledButThereIsNoClaim_ShouldReturnFalse()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions();
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsTrue(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));
		}

		[TestMethod]
		public async Task IsAutomaticallySelectedAsync_IfAutomaticSelectionIsNotEnabled_ShouldReturnFalse()
		{
			var claimsSelectionOptions = new ClaimsSelectionOptions
			{
				AutomaticSelectionEnabled = false
			};
			var claimsSelectionOptionsMonitor = await this.CreateClaimsSelectionOptionsMonitorAsync(claimsSelectionOptions);

			var claimsSelectionContext = await this.CreateClaimsSelectionContextAsync(claimsSelectionOptionsMonitor);

			Assert.IsFalse(claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled);

			var claims = new ClaimBuilderCollection();
			var claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));

			claims = new ClaimBuilderCollection
			{
				new ClaimBuilder
				{
					Type = claimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType,
					Value = true.ToString()
				}
			};
			claimsPrincipal = await ClaimsPrincipalFactory.CreateAsync(claims);
			Assert.IsFalse(await claimsSelectionContext.IsAutomaticallySelectedAsync(claimsPrincipal));
		}

		#endregion
	}
}