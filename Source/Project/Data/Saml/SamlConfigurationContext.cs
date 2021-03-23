using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Entities;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Extensions;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;

namespace HansKindberg.IdentityServer.Data.Saml
{
	public abstract class SamlConfigurationContext : DbContext, ISamlConfigurationDbContext
	{
		#region Constructors

		protected SamlConfigurationContext(DbContextOptions options) : base(options) { }

		#endregion

		#region Properties

		public virtual DbSet<ServiceProvider> ServiceProviders { get; set; }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			modelBuilder.ConfigureServiceProviderContext();

			base.OnModelCreating(modelBuilder);
		}

		public virtual async Task<int> SaveChangesAsync()
		{
			return await this.SaveChangesAsync(new CancellationToken());
		}

		#endregion
	}

	public abstract class SamlConfigurationContext<T> : SamlConfigurationContext where T : SamlConfigurationContext
	{
		#region Constructors

		protected SamlConfigurationContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}