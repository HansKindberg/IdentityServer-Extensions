using System;
using HansKindberg.IdentityServer.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HansKindberg.IdentityServer.DataProtection.Configuration
{
	public abstract class EntityFrameworkOptions<T> : ExtendedDataProtectionOptions where T : DbContext, IDataProtectionKeyContext
	{
		#region Properties

		public virtual string ConnectionStringName { get; set; } = "DataProtection";

		/// <summary>
		/// If the database-migrations should be used.
		/// </summary>
		public virtual bool Migrate { get; set; } = true;

		#endregion

		#region Methods

		protected internal override void AddInternal(IDataProtectionBuilder dataProtectionBuilder, IServiceConfiguration serviceConfiguration)
		{
			if(dataProtectionBuilder == null)
				throw new ArgumentNullException(nameof(dataProtectionBuilder));

			if(serviceConfiguration == null)
				throw new ArgumentNullException(nameof(serviceConfiguration));

			dataProtectionBuilder.Services.AddDbContext<T>(optionsBuilder => this.UseDatabase(optionsBuilder, serviceConfiguration));

			dataProtectionBuilder.PersistKeysToDbContext<T>();

			base.AddInternal(dataProtectionBuilder, serviceConfiguration);
		}

		protected internal virtual string GetConnectionString(IConfiguration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var connectionString = configuration.GetConnectionString(this.ConnectionStringName);

			if(connectionString == null)
				throw new InvalidOperationException($"The connection-string \"{this.ConnectionStringName}\" does not exist.");

			return connectionString;
		}

		protected internal abstract DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder contextOptionsBuilder, IServiceConfiguration serviceConfiguration);

		protected internal override void UseInternal(IApplicationBuilder applicationBuilder)
		{
			if(applicationBuilder == null)
				throw new ArgumentNullException(nameof(applicationBuilder));

			if(!this.Migrate)
				return;

			using(var scope = applicationBuilder.ApplicationServices.CreateScope())
			{
				scope.ServiceProvider.GetRequiredService<T>().Database.Migrate();
			}
		}

		#endregion
	}
}