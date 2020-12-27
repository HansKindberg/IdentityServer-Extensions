using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.DataProtection.Data
{
	public class SqliteDataProtectionFactory : IDesignTimeDbContextFactory<SqliteDataProtection>
	{
		#region Methods

		public virtual SqliteDataProtection CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteDataProtection>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteDataProtection(optionsBuilder.Options);
		}

		#endregion
	}
}