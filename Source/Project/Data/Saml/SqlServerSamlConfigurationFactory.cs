using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data.Saml
{
	public class SqlServerSamlConfigurationFactory : IDesignTimeDbContextFactory<SqlServerSamlConfiguration>
	{
		#region Methods

		public virtual SqlServerSamlConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqlServerSamlConfiguration>();
			optionsBuilder.UseSqlServer("A value that can not be empty just to be able to create/update migrations.");

			return new SqlServerSamlConfiguration(optionsBuilder.Options);
		}

		#endregion
	}
}