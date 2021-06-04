using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
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