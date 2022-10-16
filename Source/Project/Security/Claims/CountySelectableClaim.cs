using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using HansKindberg.IdentityServer.Security.Claims.County;
using Newtonsoft.Json;
using RegionOrebroLan.Localization.Extensions;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class CountySelectableClaim : ISelectableClaim
	{
		#region Fields

		private const char _delimiter = ':';
		private IReadOnlyDictionary<string, string> _details;
		private Lazy<string> _id;
		private Lazy<string> _text;
		private Lazy<string> _value;

		#endregion

		#region Constructors

		public CountySelectableClaim(string claimTypePrefix, string key, bool presentEmployeeHsaId, Selection selection)
		{
			this.ClaimTypePrefix = claimTypePrefix;
			this.Key = key ?? throw new ArgumentNullException(nameof(key));
			this.PresentEmployeeHsaId = presentEmployeeHsaId;
			this.Selection = selection ?? throw new ArgumentNullException(nameof(selection));
		}

		#endregion

		#region Properties

		protected internal virtual string ClaimTypePrefix { get; }
		public virtual char Delimiter => _delimiter;

		public virtual IReadOnlyDictionary<string, string> Details
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._details == null)
				{
					var details = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
					{
						{ nameof(Commission.EmployeeHsaId), this.Selection.EmployeeHsaId }
					};

					if(this.Selection.Commission != null)
					{
						details.Add(nameof(Commission.CommissionName), this.Selection.Commission.CommissionName);
						details.Add(nameof(Commission.HealthCareUnitName), this.Selection.Commission.HealthCareUnitName);
						details.Add(nameof(Commission.CommissionPurpose), this.Selection.Commission.CommissionPurpose);
						details.Add(nameof(Commission.HealthCareProviderName), this.Selection.Commission.HealthCareProviderName);
					}

					this._details = new ReadOnlyDictionary<string, string>(details);
				}
				// ReSharper restore InvertIf

				return this._details;
			}
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		public virtual string Id
		{
			get
			{
				this._id ??= new Lazy<string>(() => $"{this.Key}-{this.Value.Replace(this.Delimiter, '-')}".ToLowerInvariant());

				return this._id.Value;
			}
		}

		protected internal virtual string Key { get; }
		protected internal virtual bool PresentEmployeeHsaId { get; }
		public virtual bool Selected { get; set; }
		public virtual Selection Selection { get; }

		public virtual string Text
		{
			get
			{
				this._text ??= new Lazy<string>(() =>
				{
					var parts = new List<string>();

					if(this.PresentEmployeeHsaId)
						parts.Add(this.Selection.EmployeeHsaId);

					if(this.Selection.Commission != null)
						parts.Add(this.Selection.Commission.CommissionName);

					return string.Join($"{this.Delimiter} ", parts);
				});

				return this._text.Value;
			}
		}

		public virtual string Value
		{
			get
			{
				this._value ??= new Lazy<string>(() =>
				{
					var parts = new List<string>
					{
						this.Selection.EmployeeHsaId
					};

					if(this.Selection.Commission != null)
						parts.Add(this.Selection.Commission.CommissionHsaId);

					return string.Join(this.Delimiter, parts);
				});

				return this._value.Value;
			}
		}

		#endregion

		#region Methods

		public virtual IClaimBuilderCollection Build()
		{
			var claims = new ClaimBuilderCollection();

			this.PopulateClaims(claims, nameof(Commission.CommissionHsaId), this.Selection.Commission?.CommissionHsaId);
			this.PopulateClaims(claims, nameof(Commission.CommissionName), this.Selection.Commission?.CommissionName);
			this.PopulateClaims(claims, nameof(Commission.CommissionPurpose), this.Selection.Commission?.CommissionPurpose);

			if(this.Selection.Commission != null)
			{
				foreach(var commissionRigth in this.Selection.Commission.CommissionRights)
				{
					this.PopulateClaims(claims, nameof(CommissionRight), JsonConvert.SerializeObject(commissionRigth));
				}
			}

			this.PopulateClaims(claims, nameof(Commission.EmployeeHsaId), this.Selection.EmployeeHsaId);
			this.PopulateClaims(claims, nameof(Selection.GivenName), this.Selection.GivenName);

			this.PopulateClaims(claims, nameof(Commission.HealthCareProviderHsaId), this.Selection.Commission?.HealthCareProviderHsaId);
			this.PopulateClaims(claims, nameof(Commission.HealthCareProviderName), this.Selection.Commission?.HealthCareProviderName);
			this.PopulateClaims(claims, nameof(Commission.HealthCareProviderOrgNo), this.Selection.Commission?.HealthCareProviderOrgNo);
			this.PopulateClaims(claims, nameof(Commission.HealthCareUnitHsaId), this.Selection.Commission?.HealthCareUnitHsaId);
			this.PopulateClaims(claims, nameof(Commission.HealthCareUnitName), this.Selection.Commission?.HealthCareUnitName);
			this.PopulateClaims(claims, nameof(Commission.HealthCareUnitStartDate), JsonConvert.SerializeObject(this.Selection.Commission?.HealthCareUnitStartDate));

			this.PopulateClaims(claims, nameof(Selection.Mail), this.Selection.Mail);
			this.PopulateClaims(claims, nameof(Selection.PaTitleCode), this.Selection.PaTitleCode);
			this.PopulateClaims(claims, nameof(Selection.PersonalIdentityNumber), this.Selection.PersonalIdentityNumber);
			this.PopulateClaims(claims, nameof(Selection.PersonalPrescriptionCode), this.Selection.PersonalPrescriptionCode);
			this.PopulateClaims(claims, nameof(Selection.Surname), this.Selection.Surname);
			this.PopulateClaims(claims, nameof(Selection.SystemRole), this.Selection.SystemRole);

			return claims;
		}

		protected internal virtual void PopulateClaims(IClaimBuilderCollection claims, string propertyName, string value)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			if(string.IsNullOrEmpty(value))
				return;

			var claim = new ClaimBuilder { Type = $"{this.ClaimTypePrefix}{propertyName.FirstCharacterToLowerInvariant()}", Value = value };

			claims.Add(claim);
		}

		#endregion
	}
}