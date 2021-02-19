using System;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.DataProtection.Data
{
	public class SqliteDataProtection : DataProtectionContext<SqliteDataProtection>
	{
		#region Constructors

		public SqliteDataProtection(DbContextOptions<SqliteDataProtection> options) : base(options) { }

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