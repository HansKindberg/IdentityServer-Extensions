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
		protected internal virtual char Delimiter => _delimiter;

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
		protected internal virtual Selection Selection { get; }

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

		public virtual IDictionary<string, IClaimBuilderCollection> Build()
		{
			var claimsDictionary = new SortedDictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase);

			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.CommissionHsaId), this.Selection.Commission?.CommissionHsaId);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.CommissionName), this.Selection.Commission?.CommissionName);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.CommissionPurpose), this.Selection.Commission?.CommissionPurpose);

			var commissionRightClaims = new ClaimBuilderCollection();
			var commissionRightType = this.GetClaimType(nameof(CommissionRight));

			if(this.Selection.Commission != null)
			{
				foreach(var commissionRigth in this.Selection.Commission.CommissionRights)
				{
					this.PopulateClaims(commissionRightClaims, commissionRightType, JsonConvert.SerializeObject(commissionRigth));
				}
			}

			claimsDictionary.Add(commissionRightType, commissionRightClaims);

			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.EmployeeHsaId), this.Selection.EmployeeHsaId);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.GivenName), this.Selection.GivenName);

			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareProviderHsaId), this.Selection.Commission?.HealthCareProviderHsaId);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareProviderName), this.Selection.Commission?.HealthCareProviderName);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareProviderOrgNo), this.Selection.Commission?.HealthCareProviderOrgNo);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareUnitHsaId), this.Selection.Commission?.HealthCareUnitHsaId);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareUnitName), this.Selection.Commission?.HealthCareUnitName);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Commission.HealthCareUnitStartDate), JsonConvert.SerializeObject(this.Selection.Commission?.HealthCareUnitStartDate));

			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.Mail), this.Selection.Mail);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.PaTitleCode), this.Selection.PaTitleCode);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.PersonalIdentityNumber), this.Selection.PersonalIdentityNumber);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.PersonalPrescriptionCode), this.Selection.PersonalPrescriptionCode);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.Surname), this.Selection.Surname);
			this.PopulateClaimsDictionaryByPropertyName(claimsDictionary, nameof(Selection.SystemRole), this.Selection.SystemRole);

			return claimsDictionary;
		}

		protected internal virtual string GetClaimType(string propertyName)
		{
			if(propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			return $"{this.ClaimTypePrefix}{propertyName.FirstCharacterToLowerInvariant()}";
		}

		protected internal virtual void PopulateClaims(IClaimBuilderCollection claims, string type, string value)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			if(string.IsNullOrEmpty(value))
				return;

			var claim = new ClaimBuilder { Type = type, Value = value };

			claims.Add(claim);
		}

		protected internal virtual void PopulateClaimsDictionaryByClaimType(IDictionary<string, IClaimBuilderCollection> claimsDictionary, string type, string value)
		{
			if(claimsDictionary == null)
				throw new ArgumentNullException(nameof(claimsDictionary));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			var claims = new ClaimBuilderCollection();

			this.PopulateClaims(claims, type, value);

			claimsDictionary.Add(type, claims);
		}

		protected internal virtual void PopulateClaimsDictionaryByPropertyName(IDictionary<string, IClaimBuilderCollection> claimsDictionary, string propertyName, string value)
		{
			if(claimsDictionary == null)
				throw new ArgumentNullException(nameof(claimsDictionary));

			if(propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			this.PopulateClaimsDictionaryByClaimType(claimsDictionary, this.GetClaimType(propertyName), value);
		}

		#endregion
	}
}