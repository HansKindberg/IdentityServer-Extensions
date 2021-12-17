using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace HansKindberg.IdentityServer.Application.Models.Views.Diagnostics
{
	public class DataProtectionViewModel
	{
		#region Properties

		public virtual IApplicationDiscriminator ApplicationDiscriminator { get; set; }
		public virtual KeyManagementOptions KeyManagementOptions { get; set; }

		#endregion
	}
}