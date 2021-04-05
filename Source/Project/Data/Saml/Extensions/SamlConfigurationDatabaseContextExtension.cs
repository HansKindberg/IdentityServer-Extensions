using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Entities;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;

namespace HansKindberg.IdentityServer.Data.Saml.Extensions
{
	public static class SamlConfigurationDatabaseContextExtension
	{
		#region Methods

		public static IQueryable<AssertionConsumerService> AssertionConsumerServices(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return samlConfigurationDatabaseContext.ThrowIfNull().ServiceProviders.Include(serviceProvider => serviceProvider.AssertionConsumerServices).SelectMany(serviceProvider => serviceProvider.AssertionConsumerServices);
		}

		private static DbContext Casted(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return (DbContext)samlConfigurationDatabaseContext.ThrowIfNull();
		}

		public static ChangeTracker ChangeTracker(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return samlConfigurationDatabaseContext.Casted().ChangeTracker;
		}

		public static IQueryable<SamlClaimMap> ClaimsMapping(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return samlConfigurationDatabaseContext.ThrowIfNull().ServiceProviders.Include(serviceProvider => serviceProvider.ClaimsMapping).SelectMany(serviceProvider => serviceProvider.ClaimsMapping);
		}

		public static IQueryable<SigningCertificate> SigningCertificates(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return samlConfigurationDatabaseContext.ThrowIfNull().ServiceProviders.Include(serviceProvider => serviceProvider.SigningCertificates).SelectMany(serviceProvider => serviceProvider.SigningCertificates);
		}

		public static IQueryable<SingleLogoutService> SingleLogoutServices(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			return samlConfigurationDatabaseContext.ThrowIfNull().ServiceProviders.Include(serviceProvider => serviceProvider.SingleLogoutServices).SelectMany(serviceProvider => serviceProvider.SingleLogoutServices);
		}

		private static ISamlConfigurationDbContext ThrowIfNull(this ISamlConfigurationDbContext samlConfigurationDatabaseContext)
		{
			if(samlConfigurationDatabaseContext == null)
				throw new ArgumentNullException(nameof(samlConfigurationDatabaseContext));

			return samlConfigurationDatabaseContext;
		}

		#endregion
	}
}