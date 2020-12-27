using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public class SqliteIdentityFactory : IDesignTimeDbContextFactory<SqliteIdentity>
	{
		#region Methods

		public virtual SqliteIdentity CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteIdentity>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteIdentity(optionsBuilder.Options);
		}

		#endregion
	}
}