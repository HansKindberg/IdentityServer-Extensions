using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HansKindberg.IdentityServer.Application.Models.Views.Shared.Forms;

namespace HansKindberg.IdentityServer.Application.Models.Views.Consent
{
	public class ConsentForm : ReturnForm
	{
		#region Properties

		public virtual bool Accept { get; set; }
		public virtual IList<ScopeViewModel> ApiScopes { get; } = new List<ScopeViewModel>();
		public virtual IList<string> ConsentedApiScopes { get; } = new List<string>();
		public virtual IList<string> ConsentedIdentityResources { get; } = new List<string>();

		[Display(Name = "Description/Name", Prompt = "Description/Prompt")]
		[MaxLength(256)]
		public virtual string Description { get; set; }

		/// <summary>
		/// Used to add extra hidden fields. Used by the device-consent to add the user-code.
		/// </summary>
		public virtual IDictionary<string, string> Dictionary { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public virtual IList<ScopeViewModel> IdentityResources { get; } = new List<ScopeViewModel>();

		[Display(Name = "Persistent/Name")]
		public virtual bool Persistent { get; set; }

		#endregion
	}
}