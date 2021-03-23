using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data.Saml
{
	public class SqlServerSamlConfiguration : SamlConfigurationContext<SqlServerSamlConfiguration>
	{
		#region Constructors

		public SqlServerSamlConfiguration(DbContextOptions<SqlServerSamlConfiguration> options) : base(options) { }

		#endregion
	}
}