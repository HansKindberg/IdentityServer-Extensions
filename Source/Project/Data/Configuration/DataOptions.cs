using HansKindberg.IdentityServer.Configuration;

namespace HansKindberg.IdentityServer.Data.Configuration
{
	public class DataOptions
	{
		#region Properties

		public virtual string ConnectionStringName { get; set; } = ConfigurationKeys.DefaultConnectionStringName;
		public virtual DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;

		#endregion
	}
}