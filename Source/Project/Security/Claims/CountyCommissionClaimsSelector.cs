using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RegionOrebroLan.Localization.Extensions;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// Commission (only) claims-selector. The employeeHsaId-claim must have been selected ad the County IdP.
	/// </summary>
	/// <inheritdoc />
	public class CountyCommissionClaimsSelector : ClaimsSelector
	{
		#region Fields

		private const string _group = "Commission";

		#endregion

		#region Constructors

		public CountyCommissionClaimsSelector(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		}

		#endregion

		#region Properties

		public virtual string AllCommissionsClaimType { get; set; } = "allCommissions";
		protected internal virtual string Group => _group;
		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }

		#endregion

		#region Methods

		protected internal virtual async Task DetermineSelectedFromClaimsAsync(ClaimsPrincipal claimsPrincipal, IList<CountySelectableClaim> selectables)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selectables == null)
				throw new ArgumentNullException(nameof(selectables));

			await Task.CompletedTask.ConfigureAwait(false);

			if(selectables.Any(selectable => selectable.Selected))
				return;

			var employeeHsaIdClaim = claimsPrincipal.FindFirst(nameof(Commission.EmployeeHsaId).FirstCharacterToLowerInvariant());

			if(employeeHsaIdClaim == null)
				return;

			Commission commission = null;
			var employeeHsaId = employeeHsaIdClaim.Value;

			var commissionHsaIdClaim = claimsPrincipal.FindFirst(nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant());

			if(commissionHsaIdClaim != null)
				commission = new Commission { CommissionHsaId = commissionHsaIdClaim.Value };

			var selectableCompare = new CountySelectableClaim(commission, employeeHsaId, this.Group);

			var selectedSelectable = selectables.FirstOrDefault(selectable => string.Equals(selectable.Value, selectableCompare.Value, StringComparison.OrdinalIgnoreCase));

			if(selectedSelectable == null)
				return;

			selectedSelectable.Selected = true;
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		protected internal virtual async Task<IList<Commission>> GetAllCommissionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			IList<Commission> commissions = null;

			var allCommissionsJson = claimsPrincipal.FindFirst(this.AllCommissionsClaimType)?.Value;

			if(!string.IsNullOrWhiteSpace(allCommissionsJson))
			{
				try
				{
					commissions = JsonConvert.DeserializeObject<List<Commission>>(allCommissionsJson);
				}
				catch(Exception exception)
				{
					this.Logger.LogErrorIfEnabled(exception, $"Could not deserialize the following json to a commission-list: {allCommissionsJson}");

					commissions = null;
				}
			}

			commissions ??= new List<Commission>();

			return await Task.FromResult(commissions).ConfigureAwait(false);
		}

		public override async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(IClaimsSelectionResult selectionResult)
		{
			var claimsInformation = await this.GetClaimsInformationAsync(selectionResult).ConfigureAwait(false);

			return claimsInformation.Item1;
		}

		protected internal virtual async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, Commission commission)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var claims = new Dictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase);

			var allCommissionsClaim = claimsPrincipal.FindFirst(this.AllCommissionsClaimType);
			var issuer = allCommissionsClaim?.Issuer;
			var originalIssuer = allCommissionsClaim?.OriginalIssuer;

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant(), commission?.CommissionHsaId).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionName).FirstCharacterToLowerInvariant(), commission?.CommissionName).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionPurpose).FirstCharacterToLowerInvariant(), commission?.CommissionPurpose).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(CommissionRight).FirstCharacterToLowerInvariant(), null).ConfigureAwait(false);

			if(commission != null)
			{
				foreach(var commissionRigth in commission.CommissionRights)
				{
					await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(CommissionRight).FirstCharacterToLowerInvariant(), JsonConvert.SerializeObject(commissionRigth)).ConfigureAwait(false);
				}
			}

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderHsaId).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderHsaId).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderName).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderName).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderOrgNo).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderOrgNo).ConfigureAwait(false);

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareUnitHsaId).FirstCharacterToLowerInvariant(), commission?.HealthCareUnitHsaId).ConfigureAwait(false);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareUnitName).FirstCharacterToLowerInvariant(), commission?.HealthCareUnitName).ConfigureAwait(false);

			return claims;
		}

		protected internal virtual async Task<Tuple<IDictionary<string, IClaimBuilderCollection>, ClaimsPrincipal, CountySelectableClaim>> GetClaimsInformationAsync(IClaimsSelectionResult selectionResult)
		{
			if(selectionResult == null)
				throw new ArgumentNullException(nameof(selectionResult));

			if(!ReferenceEquals(this, selectionResult.Selector))
				throw new ArgumentNullException(nameof(selectionResult), "The selector-property is invalid.");

			if(!selectionResult.Complete)
				throw new InvalidOperationException("The selection is not complete.");

			var httpContext = this.HttpContextAccessor.HttpContext;

			if(httpContext == null)
				throw new InvalidOperationException("The http-context is null.");

			if(!httpContext.User.IsAuthenticated())
				throw new InvalidOperationException("The http-context-user is not authenticated.");

			if(!selectionResult.Selectables.TryGetValue(this.Group, out var selectables))
				throw new InvalidOperationException($"There is no selectable with key \"{this.Group}\".");

			var selectedSelectable = selectables.FirstOrDefault(selectable => selectable.Selected);

			if(selectedSelectable == null)
				throw new InvalidOperationException("There is no selected selectable.");

			if(selectedSelectable is not CountySelectableClaim countySelectableClaim)
				throw new InvalidOperationException($"The selected selectable must be of type \"{typeof(CountySelectableClaim)}\".");

			var claims = await this.GetClaimsAsync(httpContext.User, countySelectableClaim.Commission).ConfigureAwait(false);

			return Tuple.Create(claims, httpContext.User, countySelectableClaim);
		}

		protected internal virtual async Task<IList<KeyValuePair<string, Commission>>> GetCommissionMapAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var commissionMap = new List<KeyValuePair<string, Commission>>();

			var employeeHsaId = claimsPrincipal.FindFirst(nameof(Commission.EmployeeHsaId).FirstCharacterToLowerInvariant())?.Value;

			// ReSharper disable All
			if(!string.IsNullOrEmpty(employeeHsaId))
			{
				var allCommissions = await this.GetAllCommissionsAsync(claimsPrincipal).ConfigureAwait(false);

				var commissions = allCommissions.Where(commission => string.Equals(commission.EmployeeHsaId, employeeHsaId, StringComparison.OrdinalIgnoreCase)).ToArray();

				foreach(var commission in commissions)
				{
					commissionMap.Add(new KeyValuePair<string, Commission>(employeeHsaId, commission));
				}
			}
			// ReSharper restore All

			return commissionMap;
		}

		protected internal virtual async Task PopulateClaimsAsync(IDictionary<string, IClaimBuilderCollection> claims, string issuer, string originalIssuer, string type, string value)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			await Task.CompletedTask.ConfigureAwait(false);

			if(!claims.ContainsKey(type))
				claims.Add(type, new ClaimBuilderCollection());

			if(value == null)
				return;

			var collection = claims[type];

			collection.Add(new ClaimBuilder
			{
				Issuer = issuer,
				OriginalIssuer = originalIssuer,
				Type = type,
				Value = value
			});
		}

		public override async Task<IClaimsSelectionResult> SelectAsync(IDictionary<string, string> selections)
		{
			if(selections == null)
				throw new ArgumentNullException(nameof(selections));

			var httpContext = this.HttpContextAccessor.HttpContext;

			if(httpContext == null)
				throw new InvalidOperationException("The http-context is null.");

			if(!httpContext.User.IsAuthenticated())
				throw new InvalidOperationException("The http-context-user is not authenticated.");

			var result = new ClaimsSelectionResult(this);
			var selectables = new List<CountySelectableClaim>();

			foreach(var (employeeHsaId, commission) in await this.GetCommissionMapAsync(httpContext.User).ConfigureAwait(false))
			{
				var selectable = new CountySelectableClaim(commission, employeeHsaId, this.Group);

				if(selections.ContainsKey(this.Group))
					selectable.Selected = string.Equals(selections[this.Group], selectable.Value, StringComparison.OrdinalIgnoreCase);

				selectables.Add(selectable);
			}

			if(!selectables.Any(selectable => selectable.Selected))
				await this.DetermineSelectedFromClaimsAsync(httpContext.User, selectables).ConfigureAwait(false);

			if(selectables.Any())
				result.Selectables.Add(this.Group, new List<ISelectableClaim>(selectables));

			return result;
		}

		#endregion
	}
}