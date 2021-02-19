using System;
using HansKindberg.IdentityServer.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.Identity.Data
{
	public class SqliteIdentity : IdentityContext<SqliteIdentity>
	{
		#region Constructors

		public SqliteIdentity(DbContextOptions<SqliteIdentity> options) : base(options) { }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder builder)
		{
			if(builder == null)
				throw new ArgumentNullException(nameof(builder));

			base.OnModelCreating(builder);

			builder.SqliteCaseInsensitive();
		}

		#endregion
	}
}