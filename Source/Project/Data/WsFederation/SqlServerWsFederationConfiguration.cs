using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data.WsFederation
{
	public class SqlServerWsFederationConfiguration : WsFederationConfigurationContext<SqlServerWsFederationConfiguration>
	{
		#region Constructors

		public SqlServerWsFederationConfiguration(DbContextOptions<SqlServerWsFederationConfiguration> options) : base(options) { }

		#endregion
	}
}