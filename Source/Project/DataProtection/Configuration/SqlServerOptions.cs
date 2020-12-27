using System;
using HansKindberg.IdentityServer.DataProtection.Data;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public class SqlServerOptions : EntityFrameworkOptions<SqlServerDataProtection>
	{
		#region Methods

		protected internal override DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder contextOptionsBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			return contextOptionsBuilder.UseSqlServer(this.GetConnectionString(serviceConfiguration.Configuration));
		}

		#endregion
	}
}