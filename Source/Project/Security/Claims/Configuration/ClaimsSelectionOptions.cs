using System;
using System.Collections.Generic;
using HansKindberg.IdentityServer.FeatureManagement;

namespace HansKindberg.IdentityServer.Security.Claims.Configuration
{
	public class ClaimsSelectionOptions
	{
		#region Properties

		public virtual string AutomaticSelectionClaimType { get; set; } = "automatic_claims_selection";

		/// <summary>
		/// If enabled, automatic selection should be performed if there is only one selection and selection is required.
		/// </summary>
		public virtual bool AutomaticSelectionEnabled { get; set; } = true;

		public virtual string DefaultReturnUrl { get; set; } = "/Account";

		/// <summary>
		/// The path to the claims-selection handler (controller/action/page).
		/// </summary>
		public virtual string Path { get; set; } = $"/{nameof(Feature.ClaimsSelection)}";

		public virtual IDictionary<string, ClaimsSelectorOptions> Selectors { get; } = new Dictionary<string, ClaimsSelectorOptions>(StringComparer.OrdinalIgnoreCase);

		#endregion
	}
}