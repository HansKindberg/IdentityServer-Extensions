using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public class SqlServerIdentity : IdentityContext<SqlServerIdentity>
	{
		#region Constructors

		public SqlServerIdentity(DbContextOptions<SqlServerIdentity> options) : base(options) { }

		#endregion
	}
}