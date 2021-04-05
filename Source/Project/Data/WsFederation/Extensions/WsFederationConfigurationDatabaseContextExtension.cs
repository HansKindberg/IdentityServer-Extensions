using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Entities;

namespace HansKindberg.IdentityServer.Data.WsFederation.Extensions
{
	public static class WsFederationConfigurationDatabaseContextExtension
	{
		#region Methods

		private static DbContext Casted(this IWsFederationConfigurationDbContext wsFederationConfigurationDatabaseContext)
		{
			return (DbContext)wsFederationConfigurationDatabaseContext.ThrowIfNull();
		}

		public static ChangeTracker ChangeTracker(this IWsFederationConfigurationDbContext wsFederationConfigurationDatabaseContext)
		{
			return wsFederationConfigurationDatabaseContext.Casted().ChangeTracker;
		}

		public static IQueryable<WsFederationClaimMap> ClaimMapping(this IWsFederationConfigurationDbContext wsFederationConfigurationDatabaseContext)
		{
			return wsFederationConfigurationDatabaseContext.ThrowIfNull().RelyingParties.Include(relyingParty => relyingParty.ClaimMapping).SelectMany(relyingParty => relyingParty.ClaimMapping);
		}

		private static IWsFederationConfigurationDbContext ThrowIfNull(this IWsFederationConfigurationDbContext wsFederationConfigurationDatabaseContext)
		{
			if(wsFederationConfigurationDatabaseContext == null)
				throw new ArgumentNullException(nameof(wsFederationConfigurationDatabaseContext));

			return wsFederationConfigurationDatabaseContext;
		}

		#endregion
	}
}