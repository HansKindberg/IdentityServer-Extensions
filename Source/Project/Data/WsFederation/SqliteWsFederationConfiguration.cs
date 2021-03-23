using System;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data.WsFederation
{
	public class SqliteWsFederationConfiguration : WsFederationConfigurationContext<SqliteWsFederationConfiguration>
	{
		#region Constructors

		public SqliteWsFederationConfiguration(DbContextOptions<SqliteWsFederationConfiguration> options) : base(options) { }

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