using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Security.Claims.County
{
	public class Commission
	{
		#region Properties

		public virtual string CommissionHsaId { get; set; }
		public virtual string CommissionName { get; set; }
		public virtual string CommissionPurpose { get; set; }
		public virtual IList<CommissionRight> CommissionRights { get; } = new List<CommissionRight>();
		public virtual string EmployeeHsaId { get; set; }
		public virtual string HealthCareProviderHsaId { get; set; }
		public virtual string HealthCareProviderName { get; set; }
		public virtual string HealthCareProviderOrgNo { get; set; }
		public virtual string HealthCareUnitHsaId { get; set; }
		public virtual string HealthCareUnitName { get; set; }
		public virtual CountyDateTime HealthCareUnitStartDate { get; set; }

		#endregion
	}
}