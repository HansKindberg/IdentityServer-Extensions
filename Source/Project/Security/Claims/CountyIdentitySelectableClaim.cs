using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class CountyIdentitySelectableClaim : ISelectableClaim
	{
		#region Fields

		private Lazy<string> _id;

		#endregion

		#region Constructors

		public CountyIdentitySelectableClaim(string identity, string key, string selectedEmployeeHsaIdClaimType)
		{
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
			this.SelectedEmployeeHsaIdClaimType = selectedEmployeeHsaIdClaimType ?? throw new ArgumentNullException(nameof(selectedEmployeeHsaIdClaimType));
		}

		#endregion

		#region Properties

		public virtual IReadOnlyDictionary<string, string> Details { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		public virtual string Id
		{
			get
			{
				this._id ??= new Lazy<string>(() => $"{this.Key}-{this.Value}".ToLowerInvariant());

				return this._id.Value;
			}
		}

		public virtual string Identity { get; }
		protected internal virtual string Key { get; }
		public virtual bool Selected { get; set; }
		protected internal virtual string SelectedEmployeeHsaIdClaimType { get; }
		public virtual string Text => this.Identity;
		public virtual string Value => this.Identity;

		#endregion

		#region Methods

		public virtual IDictionary<string, IClaimBuilderCollection> Build()
		{
			var claim = new ClaimBuilder { Type = this.SelectedEmployeeHsaIdClaimType, Value = this.Identity };

			var claims = new ClaimBuilderCollection
			{
				claim
			};

			var claimsDictionary = new SortedDictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase)
			{
				{ claim.Type, claims }
			};

			return claimsDictionary;
		}

		#endregion
	}
}