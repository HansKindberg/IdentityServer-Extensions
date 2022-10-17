using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Localization.Extensions;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public abstract class ClaimBasedCountySelectorBase : CountySelectorBase<CountySelectableClaim>
	{
		#region Constructors

		protected ClaimBasedCountySelectorBase(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		protected internal virtual async Task DetermineSelectedFromClaimsAsync(ClaimsPrincipal claimsPrincipal, IList<CountySelectableClaim> selectables)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selectables == null)
				throw new ArgumentNullException(nameof(selectables));

			await Task.CompletedTask.ConfigureAwait(false);

			if(selectables.Any(selectable => selectable.Selected))
				return;

			var selectedEmployeeHsaId = await this.GetSelectedEmployeeHsaIdAsync(claimsPrincipal).ConfigureAwait(false);

			if(selectedEmployeeHsaId == null)
				return;

			var selectedCommissionHsaIdClaim = claimsPrincipal.FindFirst($"{this.SelectedClaimTypePrefix}{nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant()}");

			if(selectedCommissionHsaIdClaim == null)
				return;

			var selectedCommissionHsaId = selectedCommissionHsaIdClaim.Value;

			var selectionCompare = new Selection
			{
				Commission = new Commission
				{
					CommissionHsaId = selectedCommissionHsaId,
					EmployeeHsaId = selectedEmployeeHsaId
				},
				EmployeeHsaId = selectedEmployeeHsaId
			};

			var selectableCompare = new CountySelectableClaim(this.SelectedClaimTypePrefix, this.Key, false, selectionCompare);

			var selectedSelectable = selectables.FirstOrDefault(selectable => string.Equals(selectable.Id, selectableCompare.Id, StringComparison.OrdinalIgnoreCase));

			if(selectedSelectable == null)
				return;

			selectedSelectable.Selected = true;
		}

		protected internal abstract Task<IList<Selection>> GetSelectionsAsync(ClaimsPrincipal claimsPrincipal);

		public override async Task<IClaimsSelectionResult> SelectAsync(ClaimsPrincipal claimsPrincipal, IDictionary<string, string> selections)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selections == null)
				throw new ArgumentNullException(nameof(selections));

			var employeeHsaIds = await this.GetEmployeeHsaIdsAsync(claimsPrincipal).ConfigureAwait(false);
			var result = new ClaimsSelectionResult(this);
			var selectables = new List<CountySelectableClaim>();

			foreach(var selection in await this.GetSelectionsAsync(claimsPrincipal).ConfigureAwait(false))
			{
				var selectable = new CountySelectableClaim(this.SelectedClaimTypePrefix, this.Key, employeeHsaIds.Count > 1, selection);

				if(selections.ContainsKey(this.Key))
					selectable.Selected = string.Equals(selections[this.Key], selectable.Value, StringComparison.OrdinalIgnoreCase);

				selectables.Add(selectable);
			}

			if(!selectables.Any(selectable => selectable.Selected))
				await this.DetermineSelectedFromClaimsAsync(claimsPrincipal, selectables).ConfigureAwait(false);

			if(selectables.Any())
				result.Selectables.Add(this.Key, new List<ISelectableClaim>(selectables));

			return result;
		}

		#endregion
	}
}