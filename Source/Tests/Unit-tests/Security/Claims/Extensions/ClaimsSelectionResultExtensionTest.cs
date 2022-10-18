using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests.Mocks.Security.Claims;

namespace UnitTests.Security.Claims.Extensions
{
	[TestClass]
	public class ClaimsSelectionResultExtensionTest
	{
		#region Methods

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test01()
		{
			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(0);

			Assert.IsFalse(await claimsSelectionResult.AnySelectableClaimsAsync());
		}

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test02()
		{
			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(1);

			Assert.IsTrue(await claimsSelectionResult.AnySelectableClaimsAsync());
		}

		[TestMethod]
		public async Task AnySelectableClaimsAsync_Test03()
		{
			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(3);

			Assert.IsTrue(await claimsSelectionResult.AnySelectableClaimsAsync());
		}

		protected internal virtual async Task<IClaimsSelectionResult> CreateClaimsSelectionResultAsync(IDictionary<string, IList<ISelectableClaim>> selectables)
		{
			var claimsSelectionResultMock = new Mock<IClaimsSelectionResult>();

			claimsSelectionResultMock.Setup(claimsSelectionResult => claimsSelectionResult.Selectables).Returns(selectables);

			return await Task.FromResult(claimsSelectionResultMock.Object);
		}

		protected internal virtual async Task<ClaimsSelectionResultMock> CreateClaimsSelectionResultMockAsync(byte numberOfSelectableClaims, byte numberOfSelectables = 1)
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

			return await Task.FromResult(claimsSelectionResult);
		}

		protected internal virtual async Task<ISelectableClaim> CreateSelectableClaimAsync(bool selected = false)
		{
			var selectableClaimMock = new Mock<ISelectableClaim>();

			selectableClaimMock.Setup(selectableClaim => selectableClaim.Selected).Returns(selected);

			return await Task.FromResult(selectableClaimMock.Object);
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test01()
		{
			const int numberOfSelectableClaims = 0;

			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(numberOfSelectableClaims);

			Assert.AreEqual(numberOfSelectableClaims, await claimsSelectionResult.NumberOfSelectableClaimsAsync());
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test02()
		{
			const int numberOfSelectableClaims = 1;

			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(numberOfSelectableClaims);

			Assert.AreEqual(numberOfSelectableClaims, await claimsSelectionResult.NumberOfSelectableClaimsAsync());
		}

		[TestMethod]
		public async Task NumberOfSelectableClaimsAsync_Test03()
		{
			const int numberOfSelectableClaims = 10;

			var claimsSelectionResult = await this.CreateClaimsSelectionResultMockAsync(numberOfSelectableClaims);

			Assert.AreEqual(numberOfSelectableClaims, await claimsSelectionResult.NumberOfSelectableClaimsAsync());
		}

		[TestMethod]
		public async Task Selected_IfManySelectedSelectableClaims_ShouldReturnTrue()
		{
			var selectableClaims1 = new List<ISelectableClaim>
			{
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync(true)
			};

			var selectableClaims2 = new List<ISelectableClaim>
			{
				await this.CreateSelectableClaimAsync(true),
				await this.CreateSelectableClaimAsync(true),
				await this.CreateSelectableClaimAsync()
			};

			var selectables = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "1", selectableClaims1 },
				{ "2", selectableClaims2 }
			};

			var selectionResult = await this.CreateClaimsSelectionResultAsync(selectables);

			Assert.IsTrue(selectionResult.Selected());
		}

		[TestMethod]
		public async Task Selected_IfNoSelectedSelectableClaim_ShouldReturnFalse()
		{
			var selectableClaims = new List<ISelectableClaim>
			{
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync()
			};

			var selectables = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "1", selectableClaims }
			};

			var selectionResult = await this.CreateClaimsSelectionResultAsync(selectables);

			Assert.IsFalse(selectionResult.Selected());

			selectables.Add("2", selectableClaims);
			selectables.Add("3", selectableClaims);

			selectionResult = await this.CreateClaimsSelectionResultAsync(selectables);

			Assert.IsFalse(selectionResult.Selected());
		}

		[TestMethod]
		public async Task Selected_IfOneSelectedSelectableClaim_ShouldReturnTrue()
		{
			var selectableClaims1 = new List<ISelectableClaim>
			{
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync()
			};

			var selectableClaims2 = new List<ISelectableClaim>
			{
				await this.CreateSelectableClaimAsync(),
				await this.CreateSelectableClaimAsync(true),
				await this.CreateSelectableClaimAsync()
			};

			var selectables = new Dictionary<string, IList<ISelectableClaim>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "1", selectableClaims1 },
				{ "2", selectableClaims2 }
			};

			var selectionResult = await this.CreateClaimsSelectionResultAsync(selectables);

			Assert.IsTrue(selectionResult.Selected());
		}

		#endregion
	}
}