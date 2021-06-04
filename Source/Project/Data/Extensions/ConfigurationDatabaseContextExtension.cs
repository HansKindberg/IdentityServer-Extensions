using System;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HansKindberg.IdentityServer.Data.Extensions
{
	public static class ConfigurationDatabaseContextExtension
	{
		#region Methods

		public static IQueryable<ApiResourceClaim> ApiResourceClaims(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiResources.Include(apiResource => apiResource.UserClaims).SelectMany(apiResource => apiResource.UserClaims);
		}

		public static IQueryable<ApiResourceProperty> ApiResourceProperties(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiResources.Include(apiResource => apiResource.Properties).SelectMany(apiResource => apiResource.Properties);
		}

		public static IQueryable<ApiResourceScope> ApiResourceScopes(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiResources.Include(apiResource => apiResource.Scopes).SelectMany(apiResource => apiResource.Scopes);
		}

		public static IQueryable<ApiResourceSecret> ApiResourceSecrets(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiResources.Include(apiResource => apiResource.Secrets).SelectMany(apiResource => apiResource.Secrets);
		}

		public static IQueryable<ApiScopeClaim> ApiScopeClaims(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiScopes.Include(apiScope => apiScope.UserClaims).SelectMany(apiScope => apiScope.UserClaims);
		}

		public static IQueryable<ApiScopeProperty> ApiScopeProperties(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().ApiScopes.Include(apiScope => apiScope.Properties).SelectMany(apiScope => apiScope.Properties);
		}

		private static DbContext Casted(this IConfigurationDbContext configurationDatabaseContext)
		{
			return (DbContext)configurationDatabaseContext.ThrowIfNull();
		}

		public static ChangeTracker ChangeTracker(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.Casted().ChangeTracker;
		}

		public static IQueryable<ClientClaim> ClientClaims(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.Claims).SelectMany(client => client.Claims);
		}

		public static IQueryable<ClientGrantType> ClientGrantTypes(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.AllowedGrantTypes).SelectMany(client => client.AllowedGrantTypes);
		}

		public static IQueryable<ClientIdPRestriction> ClientIdentityProviderRestrictions(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.IdentityProviderRestrictions).SelectMany(client => client.IdentityProviderRestrictions);
		}

		public static IQueryable<ClientPostLogoutRedirectUri> ClientPostLogoutRedirectUris(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.PostLogoutRedirectUris).SelectMany(client => client.PostLogoutRedirectUris);
		}

		public static IQueryable<ClientProperty> ClientProperties(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.Properties).SelectMany(client => client.Properties);
		}

		public static IQueryable<ClientRedirectUri> ClientRedirectUris(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.RedirectUris).SelectMany(client => client.RedirectUris);
		}

		public static IQueryable<ClientScope> ClientScopes(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.AllowedScopes).SelectMany(client => client.AllowedScopes);
		}

		public static IQueryable<ClientSecret> ClientSecrets(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().Clients.Include(client => client.ClientSecrets).SelectMany(client => client.ClientSecrets);
		}

		public static IQueryable<IdentityResourceClaim> IdentityResourceClaims(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().IdentityResources.Include(identityResource => identityResource.UserClaims).SelectMany(identityResource => identityResource.UserClaims);
		}

		public static IQueryable<IdentityResourceProperty> IdentityResourceProperties(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.ThrowIfNull().IdentityResources.Include(identityResource => identityResource.Properties).SelectMany(identityResource => identityResource.Properties);
		}

		public static int SaveChanges(this IConfigurationDbContext configurationDatabaseContext)
		{
			return configurationDatabaseContext.Casted().SaveChanges();
		}

		public static async Task<int> SaveChangesAsync(this IConfigurationDbContext configurationDatabaseContext)
		{
			return await configurationDatabaseContext.Casted().SaveChangesAsync();
		}

		private static IConfigurationDbContext ThrowIfNull(this IConfigurationDbContext configurationDatabaseContext)
		{
			if(configurationDatabaseContext == null)
				throw new ArgumentNullException(nameof(configurationDatabaseContext));

			return configurationDatabaseContext;
		}

		#endregion
	}
}