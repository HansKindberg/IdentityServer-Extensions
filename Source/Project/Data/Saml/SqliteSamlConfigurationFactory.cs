using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HansKindberg.IdentityServer.Data.Saml
{
	public class SqliteSamlConfigurationFactory : IDesignTimeDbContextFactory<SqliteSamlConfiguration>
	{
		#region Methods

		public virtual SqliteSamlConfiguration CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<SqliteSamlConfiguration>();
			optionsBuilder.UseSqlite("A value that can not be empty just to be able to create/update migrations.");

			return new SqliteSamlConfiguration(optionsBuilder.Options);
		}

		#endregion
	}
}