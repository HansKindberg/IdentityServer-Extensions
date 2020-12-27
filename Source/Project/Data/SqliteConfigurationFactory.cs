using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data
{
	public class SqliteConfigurationFactory : IDesignTimeDbContextFactory<SqliteConfiguration>
	{
		#region Methods

		public virtual SqliteConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteConfiguration>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteConfiguration(optionsBuilder.Options, new ConfigurationStoreOptions());
		}

		#endregion
	}
}