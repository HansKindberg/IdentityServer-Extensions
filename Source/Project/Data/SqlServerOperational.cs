using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
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