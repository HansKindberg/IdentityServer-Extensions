using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public class SqliteIdentity : IdentityContext<SqliteIdentity>
	{
		#region Constructors

		public SqliteIdentity(DbContextOptions<SqliteIdentity> options) : base(options) { }

		#endregion
	}
}