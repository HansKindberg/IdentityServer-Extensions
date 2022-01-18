using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Security.Claims.County;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RegionOrebroLan.Localization.Extensions;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication.DirectoryServices;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class CountyClaimsSelector : ClaimsSelector
	{
		#region Fields

		private const string _group = "Commission";

		#endregion

		#region Constructors

		public CountyClaimsSelector(IActiveDirectory activeDirectory, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.ActiveDirectory = activeDirectory ?? throw new ArgumentNullException(nameof(activeDirectory));
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		}

		#endregion

		#region Properties

		protected internal virtual IActiveDirectory ActiveDirectory { get; }

		/// <summary>
		/// Mapping from Active Directory attribute to claim-type.
		/// </summary>
		public virtual IDictionary<string, string> ActiveDirectoryMap { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public virtual string AllCommissionsClaimType { get; set; } = "allCommissions";
		public virtual string AllEmployeeHsaIdsClaimType { get; set; } = "allEmployeeHsaIds";
		protected internal virtual string Group => _group;
		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		public virtual string SamAccountNamePrefix { get; set; }

		#endregion

		#region Methods

		protected internal virtual async Task DetermineSelectedFromClaimsAsync(ClaimsPrincipal claimsPrincipal, IList<CountySelectableClaim> selectables)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(selectables == null)
				throw new ArgumentNullException(nameof(selectables));

			await Task.CompletedTask;

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

		[SuppressMessage("Style", "IDE0057:Use range operator")]
		protected internal virtual async Task<IClaimBuilderCollection> GetActiveDirectoryClaimsAsync(string employeeHsaId)
		{
			if(employeeHsaId == null)
				throw new ArgumentNullException(nameof(employeeHsaId));

			var activeDirectoryClaims = new ClaimBuilderCollection();

			if(!this.ActiveDirectoryMap.Any())
			{
				this.Logger.LogDebugIfEnabled("There are no Active Directory attribute to claim mappings configured. Skipping Active Directory claims.");
				return activeDirectoryClaims;
			}

			if(string.IsNullOrWhiteSpace(this.SamAccountNamePrefix))
			{
				this.Logger.LogDebugIfEnabled("The sam-account-name-prefix is null, empty or whitespaces only. Skipping Active Directory claims.");
				return activeDirectoryClaims;
			}

			if(!employeeHsaId.StartsWith(this.SamAccountNamePrefix, StringComparison.OrdinalIgnoreCase))
			{
				this.Logger.LogDebugIfEnabled($"The employee-hsa-id \"{employeeHsaId}\" does not start with sam-account-name-prefix \"{this.SamAccountNamePrefix}\". Skipping Active Directory claims.");
				return activeDirectoryClaims;
			}

			const string issuer = "Active Directory";
			var samAccountName = employeeHsaId.Substring(this.SamAccountNamePrefix.Length);

			var claimsPrincipal = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(JwtClaimTypes.Name, samAccountName) }) });

			var attributes = await this.ActiveDirectory.GetAttributesAsync(this.ActiveDirectoryMap.Keys, IdentifierKind.SamAccountName, claimsPrincipal);

			foreach(var (attributeName, claimType) in this.ActiveDirectoryMap)
			{
				if(attributes.ContainsKey(attributeName))
					activeDirectoryClaims.Add(new ClaimBuilder
					{
						Issuer = issuer,
						OriginalIssuer = issuer,
						Type = claimType,
						Value = attributes[attributeName]
					});
			}

			return activeDirectoryClaims;
		}

		protected internal virtual async Task<IList<Commission>> GetAllCommissionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var allCommissionsJson = claimsPrincipal.FindFirst(this.AllCommissionsClaimType)?.Value;

			var commissions = JsonConvert.DeserializeObject<List<Commission>>(allCommissionsJson);

			return await Task.FromResult(commissions);
		}

		protected internal virtual async Task<IList<string>> GetAllEmployeeHsaIdsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var allEmployeeHsaIds = claimsPrincipal.FindAll(this.AllEmployeeHsaIdsClaimType).Select(claim => claim.Value).ToList();

			return await Task.FromResult(allEmployeeHsaIds);
		}

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		public override async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(IClaimsSelectionResult selectionResult)
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

			if(!(selectedSelectable is CountySelectableClaim countySelectable))
				throw new InvalidOperationException($"The selected selectable must be of type \"{typeof(CountySelectableClaim)}\".");

			var claims = await this.GetClaimsAsync(httpContext.User, countySelectable.Commission, countySelectable.EmployeeHsaId);

			return claims;
		}

		protected internal virtual async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(ClaimsPrincipal claimsPrincipal, Commission commission, string employeeHsaId)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			if(employeeHsaId == null)
				throw new ArgumentNullException(nameof(employeeHsaId));

			var claims = new Dictionary<string, IClaimBuilderCollection>(StringComparer.OrdinalIgnoreCase);

			var allCommissionsClaim = claimsPrincipal.FindFirst(this.AllCommissionsClaimType);
			var issuer = allCommissionsClaim?.Issuer;
			var originalIssuer = allCommissionsClaim?.OriginalIssuer;

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.EmployeeHsaId).FirstCharacterToLowerInvariant(), employeeHsaId);

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionHsaId).FirstCharacterToLowerInvariant(), commission?.CommissionHsaId);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionName).FirstCharacterToLowerInvariant(), commission?.CommissionName);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.CommissionPurpose).FirstCharacterToLowerInvariant(), commission?.CommissionPurpose);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(CommissionRight).FirstCharacterToLowerInvariant(), null);

			if(commission != null)
			{
				foreach(var commissionRigth in commission.CommissionRights)
				{
					await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(CommissionRight).FirstCharacterToLowerInvariant(), JsonConvert.SerializeObject(commissionRigth));
				}
			}

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderHsaId).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderHsaId);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderName).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderName);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareProviderOrgNo).FirstCharacterToLowerInvariant(), commission?.HealthCareProviderOrgNo);

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareUnitHsaId).FirstCharacterToLowerInvariant(), commission?.HealthCareUnitHsaId);
			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, nameof(Commission.HealthCareUnitName).FirstCharacterToLowerInvariant(), commission?.HealthCareUnitName);

			foreach(var activeDirectoryClaimType in this.ActiveDirectoryMap.Values.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				await this.PopulateClaimsAsync(claims, null, null, activeDirectoryClaimType, null);
			}

			var activeDirectoryClaims = await this.GetActiveDirectoryClaimsAsync(employeeHsaId);

			foreach(var activeDirectoryClaim in activeDirectoryClaims)
			{
				await this.PopulateClaimsAsync(claims, activeDirectoryClaim.Issuer, activeDirectoryClaim.OriginalIssuer, activeDirectoryClaim.Type, activeDirectoryClaim.Value);
			}

			return claims;
		}

		protected internal virtual async Task<IList<KeyValuePair<string, Commission>>> GetCommissionMapAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var allCommissions = await this.GetAllCommissionsAsync(claimsPrincipal);
			var allEmployeeHsaIds = await this.GetAllEmployeeHsaIdsAsync(claimsPrincipal);

			var commissionMap = new List<KeyValuePair<string, Commission>>();

			// ReSharper disable LoopCanBeConvertedToQuery
			foreach(var employeeHsaId in allEmployeeHsaIds.OrderBy(employeeHsaId => employeeHsaId))
			{
				var commissions = allCommissions.Where(commission => string.Equals(commission.EmployeeHsaId, employeeHsaId, StringComparison.OrdinalIgnoreCase)).ToArray();

				if(!commissions.Any())
				{
					commissionMap.Add(new KeyValuePair<string, Commission>(employeeHsaId, null));

					continue;
				}

				foreach(var commission in commissions)
				{
					commissionMap.Add(new KeyValuePair<string, Commission>(employeeHsaId, commission));
				}
			}
			// ReSharper restore LoopCanBeConvertedToQuery

			return commissionMap;
		}

		protected internal virtual async Task PopulateClaimsAsync(IDictionary<string, IClaimBuilderCollection> claims, string issuer, string originalIssuer, string type, string value)
		{
			if(claims == null)
				throw new ArgumentNullException(nameof(claims));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			await Task.CompletedTask;

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

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
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

			foreach(var (employeeHsaId, commission) in await this.GetCommissionMapAsync(httpContext.User))
			{
				var selectable = new CountySelectableClaim(commission, employeeHsaId, this.Group);

				if(selections.ContainsKey(this.Group))
					selectable.Selected = string.Equals(selections[this.Group], selectable.Value, StringComparison.OrdinalIgnoreCase);

				selectables.Add(selectable);
			}

			if(!selectables.Any(selectable => selectable.Selected))
				await this.DetermineSelectedFromClaimsAsync(httpContext.User, selectables);

			if(selectables.Any())
				result.Selectables.Add(this.Group, new List<ISelectableClaim>(selectables));

			return result;
		}

		#endregion
	}
}