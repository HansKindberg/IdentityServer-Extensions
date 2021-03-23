using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data.WsFederation
{
	public class SqliteWsFederationConfigurationFactory : IDesignTimeDbContextFactory<SqliteWsFederationConfiguration>
	{
		#region Methods

		public virtual SqliteWsFederationConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteWsFederationConfiguration>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteWsFederationConfiguration(optionsBuilder.Options);
		}

		#endregion
	}
}