using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using Microsoft.Extensions.Options;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class ClaimsSelectionContext : IClaimsSelectionContext
	{
		#region Constructors

		public ClaimsSelectionContext(IOptionsMonitor<ClaimsSelectionOptions> claimsSelectionOptionsMonitor)
		{
			this.ClaimsSelectionOptionsMonitor = claimsSelectionOptionsMonitor ?? throw new ArgumentNullException(nameof(claimsSelectionOptionsMonitor));
		}

		#endregion

		#region Properties

		public virtual string AuthenticationScheme { get; set; }
		public virtual string AutomaticSelectionClaimType => this.ClaimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType;
		protected internal virtual IOptionsMonitor<ClaimsSelectionOptions> ClaimsSelectionOptionsMonitor { get; }
		IEnumerable<IClaimsSelector> IClaimsSelectionContext.Selectors => this.Selectors;
		public virtual IList<IClaimsSelector> Selectors { get; } = new List<IClaimsSelector>();
		public virtual Uri Url { get; set; }

		#endregion

		#region Methods

		public virtual async Task<bool> AutomaticSelectionIsPossibleAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(!this.ClaimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled)
				return false;

			foreach(var claimsSelector in this.Selectors)
			{
				var claimsSelectionResult = await claimsSelector.SelectAsync(claimsPrincipal, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

				// ReSharper disable All
				foreach(var selectable in claimsSelectionResult.Selectables)
				{
					var numberOfSelectableClaims = selectable.Value.Count;

					if(numberOfSelectableClaims == 0)
						continue;

					if(numberOfSelectableClaims == 1 && claimsSelector.SelectionRequired)
						continue;

					return false;
				}
				// ReSharper restore All
			}

			return true;
		}

		public virtual async Task<bool> IsAutomaticallySelectedAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(!this.ClaimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionEnabled)
				return false;

			var automaticSelectionClaims = claimsPrincipal.FindAll(this.ClaimsSelectionOptionsMonitor.CurrentValue.AutomaticSelectionClaimType).ToArray();

			if(!automaticSelectionClaims.Any())
				return false;

			foreach(var automaticSelectionClaim in automaticSelectionClaims)
			{
				if(!bool.TryParse(automaticSelectionClaim?.Value, out var automaticallySelected))
					return false;

				if(!automaticallySelected)
					return false;
			}

			return await Task.FromResult(true).ConfigureAwait(false);
		}

		#endregion
	}
}