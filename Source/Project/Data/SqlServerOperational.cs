using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data
{
	public class SqlServerOperational : PersistedGrantDbContext<SqlServerOperational>
	{
		#region Constructors

		public SqlServerOperational(DbContextOptions<SqlServerOperational> options, OperationalStoreOptions storeOptions) : base(options, storeOptions) { }

		#endregion
	}
}