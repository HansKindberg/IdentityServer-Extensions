using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.DataProtection.Data
{
	public class SqliteDataProtection : DataProtectionContext<SqliteDataProtection>
	{
		#region Constructors

		public SqliteDataProtection(DbContextOptions<SqliteDataProtection> options) : base(options) { }

		#endregion
	}
}