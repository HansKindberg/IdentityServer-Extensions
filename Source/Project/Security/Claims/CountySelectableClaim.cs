using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using HansKindberg.IdentityServer.Security.Claims.County;

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

		public CountySelectableClaim(Commission commission, string employeeHsaId, string group)
		{
			this.Commission = commission;
			this.EmployeeHsaId = employeeHsaId ?? throw new ArgumentNullException(nameof(employeeHsaId));
			this.Group = group ?? throw new ArgumentNullException(nameof(group));
		}

		#endregion

		#region Properties

		public virtual Commission Commission { get; }
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
						{ nameof(Commission.EmployeeHsaId), this.EmployeeHsaId }
					};

					if(this.Commission != null)
					{
						details.Add(nameof(Commission.CommissionName), this.Commission.CommissionName);
						details.Add(nameof(Commission.HealthCareUnitName), this.Commission.HealthCareUnitName);
						details.Add(nameof(Commission.CommissionPurpose), this.Commission.CommissionPurpose);
						details.Add(nameof(Commission.HealthCareProviderName), this.Commission.HealthCareProviderName);
					}

					this._details = new ReadOnlyDictionary<string, string>(details);
				}
				// ReSharper restore InvertIf

				return this._details;
			}
		}

		public virtual string EmployeeHsaId { get; }
		public virtual string Group { get; }

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		public virtual string Id
		{
			get
			{
				this._id ??= new Lazy<string>(() => $"{this.Group}-{this.Value.Replace(this.Delimiter, '-')}".ToLowerInvariant());

				return this._id.Value;
			}
		}

		public virtual bool Selected { get; set; }

		public virtual string Text
		{
			get
			{
				this._text ??= new Lazy<string>(() =>
				{
					var parts = new List<string>
					{
						this.EmployeeHsaId
					};

					if(this.Commission != null)
						parts.Add(this.Commission.CommissionName);

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
						this.EmployeeHsaId
					};

					if(this.Commission != null)
						parts.Add(this.Commission.CommissionHsaId);

					return string.Join(this.Delimiter, parts);
				});

				return this._value.Value;
			}
		}

		#endregion
	}
}