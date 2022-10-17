using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public abstract class CountyIdentitySelectorBase : CountySelectorBase<CountyIdentitySelectableClaim>
	{
		#region Constructors

		protected CountyIdentitySelectorBase(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Methods

		protected internal virtual async Task DetermineSelectedFromClaimsAsync(ClaimsPrincipal claimsPrincipal, IList<CountyIdentitySelectableClaim> selectables)
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

			var selectableCompare = new CountyIdentitySelectableClaim(selectedEmployeeHsaId, this.Key, this.SelectedEmployeeHsaIdClaimType);

			var selectedSelectable = selectables.FirstOrDefault(selectable => string.Equals(selectable.Id, selectableCompare.Id, StringComparison.OrdinalIgnoreCase));

			if(selectedSelectable == null)
				return;

			selectedSelectable.Selected = true;
		}

		protected internal abstract Task<IList<string>> GetIdentitiesAsync(ClaimsPrincipal claimsPrincipal);

		public override async Task<IClaimsSelectionResult> SelectAsync(ClaimsPrincipal claimsPrincipal, IDictionary<string, string> selections)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selections == null)
				throw new ArgumentNullException(nameof(selections));

			var employeeHsaIds = await this.GetEmployeeHsaIdsAsync(claimsPrincipal).ConfigureAwait(false);
			var result = new ClaimsSelectionResult(this);
			var selectables = new List<CountyIdentitySelectableClaim>();

			foreach(var identity in await this.GetIdentitiesAsync(claimsPrincipal).ConfigureAwait(false))
			{
				var selectable = new CountyIdentitySelectableClaim(identity, this.Key, this.SelectedEmployeeHsaIdClaimType);

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