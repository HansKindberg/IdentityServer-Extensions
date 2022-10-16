using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Security.Claims.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Security.Claims.Extensions
{
	[TestClass]
	public class ClaimsSelectionResultExtensionTest
	{
		#region Methods

		protected internal virtual async Task<IClaimsSelectionResult> CreateClaimsSelectionResultAsync(IDictionary<string, IList<ISelectableClaim>> selectables)
		{
			var claimsSelectionResultMock = new Mock<IClaimsSelectionResult>();

			claimsSelectionResultMock.Setup(claimsSelectionResult => claimsSelectionResult.Selectables).Returns(selectables);

			return await Task.FromResult(claimsSelectionResultMock.Object);
		}

		protected internal virtual async Task<ISelectableClaim> CreateSelectableClaimAsync(bool selected = false)
		{
			var selectableClaimMock = new Mock<ISelectableClaim>();

			selectableClaimMock.Setup(selectableClaim => selectableClaim.Selected).Returns(selected);

			return await Task.FromResult(selectableClaimMock.Object);
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