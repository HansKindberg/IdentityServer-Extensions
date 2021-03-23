using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data.WsFederation
{
	public class SqlServerWsFederationConfigurationFactory : IDesignTimeDbContextFactory<SqlServerWsFederationConfiguration>
	{
		#region Methods

		public virtual SqlServerWsFederationConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerWsFederationConfiguration>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerWsFederationConfiguration(optionsBuilder.Options);
		}

		#endregion
	}
}