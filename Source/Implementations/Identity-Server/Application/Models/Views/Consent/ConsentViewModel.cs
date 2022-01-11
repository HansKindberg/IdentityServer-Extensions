using System;
using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace HansKindberg.IdentityServer.Application.Models.Views.Consent
{
	public class ConsentViewModel
	{
		#region Fields

		private ConsentForm _form;

		#endregion

		#region Properties

		public virtual bool AllowPersistent { get; set; }
		public virtual Client Client { get; set; }

		/// <summary>
		/// Used to add extra information. Used by the device-consent to add the user-code.
		/// </summary>
		public virtual IDictionary<string, string> Dictionary { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public virtual ConsentForm Form
		{
			get => this._form ??= new ConsentForm();
			set => this._form = value;
		}

		#endregion
	}
}