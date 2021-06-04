using System;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data
{
	public class SqliteConfiguration : ConfigurationDbContext<SqliteConfiguration>
	{
		#region Constructors

		public SqliteConfiguration(DbContextOptions<SqliteConfiguration> options, ConfigurationStoreOptions storeOptions) : base(options, storeOptions) { }

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