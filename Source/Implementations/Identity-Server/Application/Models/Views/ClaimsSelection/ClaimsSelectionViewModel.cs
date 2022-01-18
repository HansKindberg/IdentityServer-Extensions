using System.Collections.Generic;
using HansKindberg.IdentityServer.Security.Claims;

namespace HansKindberg.IdentityServer.Application.Models.Views.ClaimsSelection
{
	public class ClaimsSelectionViewModel
	{
		#region Fields

		private ClaimsSelectionForm _form;

		#endregion

		#region Properties

		public virtual IList<string> Errors { get; } = new List<string>();

		public virtual ClaimsSelectionForm Form
		{
			get => this._form ??= new ClaimsSelectionForm();
			set => this._form = value;
		}

		public virtual IList<IClaimsSelectionResult> Results { get; } = new List<IClaimsSelectionResult>();
		public virtual string ReturnUrl { get; set; }

		#endregion
	}
}