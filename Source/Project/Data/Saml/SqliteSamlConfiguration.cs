using System;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Data.Saml
{
	public class SqliteSamlConfiguration : SamlConfigurationContext<SqliteSamlConfiguration>
	{
		#region Constructors

		public SqliteSamlConfiguration(DbContextOptions<SqliteSamlConfiguration> options) : base(options) { }

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