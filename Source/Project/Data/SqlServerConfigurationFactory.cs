using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data
{
	public class SqlServerConfigurationFactory : IDesignTimeDbContextFactory<SqlServerConfiguration>
	{
		#region Methods

		public virtual SqlServerConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerConfiguration>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerConfiguration(optionsBuilder.Options, new ConfigurationStoreOptions());
		}

		#endregion
	}
}