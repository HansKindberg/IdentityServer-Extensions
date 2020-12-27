using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data
{
	public class SqliteOperational : PersistedGrantDbContext<SqliteOperational>
	{
		#region Constructors

		public SqliteOperational(DbContextOptions<SqliteOperational> options, OperationalStoreOptions storeOptions) : base(options, storeOptions) { }

		#endregion
	}
}