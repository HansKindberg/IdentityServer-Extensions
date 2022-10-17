using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Security.Claims.County;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <summary>
	/// Select county-claims. Commissions comes from a claim. Other claims like PaTitleCode or SystemRole are not selectable with this selector. We need a service-based selector for that.
	/// </summary>
	/// <inheritdoc />
	public class ClaimBasedCountySelector : ClaimBasedCountySelectorBase
	{
		#region Fields

		private const string _commissionsClaimType = "commissions";

		#endregion

		#region Constructors

		public ClaimBasedCountySelector(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Properties

		public virtual string CommissionsClaimType { get; set; } = _commissionsClaimType;

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		protected internal override async Task<IList<Selection>> GetSelectionsAsync(ClaimsPrincipal claimsPrincipal)
		{
			if(claimsPrincipal == null)
				throw new ArgumentNullException(nameof(claimsPrincipal));

			var employeeHsaIds = await this.GetEmployeeHsaIdsAsync(claimsPrincipal).ConfigureAwait(false);
			var selections = new List<Selection>();

			// ReSharper disable All
			if(employeeHsaIds.Any())
			{
				foreach(var commissionsClaim in claimsPrincipal.FindAll(this.CommissionsClaimType))
				{
					var commissionsJson = commissionsClaim.Value ?? "[]";
					List<Commission> commissions;

					try
					{
						commissions = JsonConvert.DeserializeObject<List<Commission>>(commissionsJson) ?? Enumerable.Empty<Commission>().ToList();
					}
					catch(Exception exception)
					{
						commissions = new List<Commission>();
						this.Logger.LogDebugIfEnabled(exception, $"Could not create commissions from json {commissionsJson.ToStringRepresentation()}.");
					}

					foreach(var commission in commissions)
					{
						if(!employeeHsaIds.Contains(commission.EmployeeHsaId))
							continue;

						var selection = new Selection
						{
							Commission = commission,
							EmployeeHsaId = commission.EmployeeHsaId
						};

						selections.Add(selection);
					}
				}
			}
			// ReSharper restore All

			return selections;
		}

		#endregion
	}
}