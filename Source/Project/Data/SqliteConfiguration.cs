using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data
{
	public class SqliteConfiguration : ConfigurationDbContext<SqliteConfiguration>
	{
		#region Constructors

		public SqliteConfiguration(DbContextOptions<SqliteConfiguration> options, ConfigurationStoreOptions storeOptions) : base(options, storeOptions) { }

		#endregion
	}
}