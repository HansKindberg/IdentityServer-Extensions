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
	public abstract class CountySelectorBase<T> : ClaimsSelector<T> where T : ISelectableClaim
	{
		#region Fields

		private const string _employeeHsaIdClaimType = "hsa_identity";
		private const string _selectedClaimTypePrefix = "selected_";
		private string _selectedEmployeeHsaIdClaimType;

		#endregion

		#region Constructors

		protected CountySelectorBase(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Properties

		public virtual string EmployeeHsaIdClaimType { get; set; } = _employeeHsaIdClaimType;
		public virtual string SelectedClaimTypePrefix { get; set; } = _selectedClaimTypePrefix;

		protected internal virtual string SelectedEmployeeHsaIdClaimType
		{
			get
			{
				this._selectedEmployeeHsaIdClaimType ??= $"{this.SelectedClaimTypePrefix}{nameof(Commission.EmployeeHsaId).FirstCharacterToLowerInvariant()}";

				return this._selectedEmployeeHsaIdClaimType;
			}
		}

		#endregion

		#region Methods

		protected internal virtual async Task<ISet<string>> GetEmployeeHsaIdsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(this.EmployeeHsaIdClaimType == null)
				return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			var employeeHsaIds = claimsPrincipal
				.FindAll(this.EmployeeHsaIdClaimType)
				.Select(claim => claim.Value)
				.Where(value => !string.IsNullOrEmpty(value))
				.ToHashSet(StringComparer.OrdinalIgnoreCase);

			return await Task.FromResult(employeeHsaIds).ConfigureAwait(false);
		}

		protected internal virtual async Task<string> GetSelectedEmployeeHsaIdAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var selectedEmployeeHsaIdClaim = claimsPrincipal.FindFirst(this.SelectedEmployeeHsaIdClaimType);

			if(selectedEmployeeHsaIdClaim == null)
				return null;

			return await Task.FromResult(selectedEmployeeHsaIdClaim.Value).ConfigureAwait(false);
		}

		#endregion
	}
}