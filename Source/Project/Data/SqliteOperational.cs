using System;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
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

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			base.OnModelCreating(modelBuilder);

			modelBuilder.SqliteCaseInsensitive();
		}

		#endregion
	}
}