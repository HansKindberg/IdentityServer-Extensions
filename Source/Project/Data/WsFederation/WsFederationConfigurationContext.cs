using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Entities;
using Rsk.WsFederation.EntityFramework.Extensions;

namespace HansKindberg.IdentityServer.Data.WsFederation
{
	public abstract class WsFederationConfigurationContext : DbContext, IWsFederationConfigurationDbContext
	{
		#region Constructors

		protected WsFederationConfigurationContext(DbContextOptions options) : base(options) { }

		#endregion

		#region Properties

		public virtual DbSet<RelyingParty> RelyingParties { get; set; }

		#endregion

		#region Methods

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if(modelBuilder == null)
				throw new ArgumentNullException(nameof(modelBuilder));

			modelBuilder.ConfigureRelyingPartyContext();

			base.OnModelCreating(modelBuilder);
		}

		public virtual async Task<int> SaveChangesAsync()
		{
			return await this.SaveChangesAsync(new CancellationToken());
		}

		#endregion
	}

	public abstract class WsFederationConfigurationContext<T> : WsFederationConfigurationContext where T : WsFederationConfigurationContext
	{
		#region Constructors

		protected WsFederationConfigurationContext(DbContextOptions<T> options) : base(options) { }

		#endregion
	}
}