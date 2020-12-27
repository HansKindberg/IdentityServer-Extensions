using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data
{
	public class SqlServerConfiguration : ConfigurationDbContext<SqlServerConfiguration>
	{
		#region Constructors

		public SqlServerConfiguration(DbContextOptions<SqlServerConfiguration> options, ConfigurationStoreOptions storeOptions) : base(options, storeOptions) { }

		#endregion
	}
}