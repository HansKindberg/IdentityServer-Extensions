using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.DataProtection.Data
{
	public class SqlServerDataProtection : DataProtectionContext<SqlServerDataProtection>
	{
		#region Constructors

		public SqlServerDataProtection(DbContextOptions<SqlServerDataProtection> options) : base(options) { }

		#endregion
	}
}