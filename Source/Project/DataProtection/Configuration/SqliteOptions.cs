using System;
using HansKindberg.IdentityServer.DataProtection.Data;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public class SqliteOptions : EntityFrameworkOptions<SqliteDataProtection>
	{
		#region Methods

		protected internal override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder contextOptionsBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			return contextOptionsBuilder.UseSqlite(this.GetConnectionString(serviceConfiguration.Configuration));
		}

		#endregion
	}
}