using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Security.Claims;
using RegionOrebroLan.Web.Authentication.DirectoryServices;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// EmployeeHsaId and commission claims-selector.
	/// </summary>
	/// <inheritdoc />
	public class CountyClaimsSelector : CountyCommissionClaimsSelector
	{
		#region Constructors

		public CountyClaimsSelector(IActiveDirectory activeDirectory, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory) : base(httpContextAccessor, loggerFactory)
		{
			this.ActiveDirectory = activeDirectory ?? throw new ArgumentNullException(nameof(activeDirectory));
		}

		#endregion

		#region Properties

		protected internal virtual IActiveDirectory ActiveDirectory { get; }

		/// <summary>
		/// Mapping from Active Directory attribute to claim-type.
		/// </summary>
		public virtual IDictionary<string, string> ActiveDirectoryMap { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public virtual string AllEmployeeHsaIdsClaimType { get; set; } = "allEmployeeHsaIds";
		public virtual string SamAccountNamePrefix { get; set; }

		#endregion

		#region Methods

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

			var attributes = await this.ActiveDirectory.GetAttributesAsync(this.ActiveDirectoryMap.Keys, samAccountName, IdentifierKind.SamAccountName).ConfigureAwait(false);

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

		protected internal virtual async Task<IList<string>> GetAllEmployeeHsaIdsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var allEmployeeHsaIds = claimsPrincipal.FindAll(this.AllEmployeeHsaIdsClaimType).Select(claim => claim.Value).ToList();

			return await Task.FromResult(allEmployeeHsaIds).ConfigureAwait(false);
		}

		public override async Task<IDictionary<string, IClaimBuilderCollection>> GetClaimsAsync(IClaimsSelectionResult selectionResult)
		{
			var claimsInformation = await this.GetClaimsInformationAsync(selectionResult).ConfigureAwait(false);

			var claims = claimsInformation.Item1;
			var claimsPrincipal = claimsInformation.Item2;
			var employeeHsaId = claimsInformation.Item3.EmployeeHsaId;

			var allCommissionsClaim = claimsPrincipal.FindFirst(this.AllCommissionsClaimType);
			var issuer = allCommissionsClaim?.Issuer;
			var originalIssuer = allCommissionsClaim?.OriginalIssuer;

			await this.PopulateClaimsAsync(claims, issuer, originalIssuer, this.EmployeeHsaIdClaimType, employeeHsaId).ConfigureAwait(false);

			foreach(var activeDirectoryClaimType in this.ActiveDirectoryMap.Values.Distinct(StringComparer.OrdinalIgnoreCase))
			{
				await this.PopulateClaimsAsync(claims, null, null, activeDirectoryClaimType, null).ConfigureAwait(false);
			}

			var activeDirectoryClaims = await this.GetActiveDirectoryClaimsAsync(employeeHsaId).ConfigureAwait(false);

			foreach(var activeDirectoryClaim in activeDirectoryClaims)
			{
				await this.PopulateClaimsAsync(claims, activeDirectoryClaim.Issuer, activeDirectoryClaim.OriginalIssuer, activeDirectoryClaim.Type, activeDirectoryClaim.Value).ConfigureAwait(false);
			}

			return claims;
		}

		protected internal override async Task<IList<KeyValuePair<string, Commission>>> GetCommissionMapAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var allCommissions = await this.GetAllCommissionsAsync(claimsPrincipal).ConfigureAwait(false);
			var allEmployeeHsaIds = await this.GetAllEmployeeHsaIdsAsync(claimsPrincipal).ConfigureAwait(false);

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

		#endregion
	}
}