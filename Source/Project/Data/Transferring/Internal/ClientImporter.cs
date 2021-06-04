using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Validation;
using HansKindberg.IdentityServer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClientEntity = Duende.IdentityServer.EntityFramework.Entities.Client;
using ClientModel = Duende.IdentityServer.Models.Client;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class ClientImporter : ConfigurationEntityImporter<ClientEntity, ClientModel>
	{
		#region Constructors

		public ClientImporter(IClientConfigurationValidator clientValidator, IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(databaseContext, loggerFactory)
		{
			this.ClientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
		}

		#endregion

		#region Properties

		protected internal virtual IClientConfigurationValidator ClientValidator { get; }
		protected internal override DbSet<ClientEntity> Entities => this.DatabaseContext.Clients;
		protected internal override Func<ClientEntity, string> EntityIdentifierSelector => entity => entity.ClientId;
		protected internal override string ModelIdentifierName => nameof(ClientModel.ClientId);
		protected internal override Func<ClientModel, string> ModelIdentifierSelector => model => model.ClientId;
		protected internal override Func<ClientModel, ClientEntity> ModelToEntityFunction => model => model.ToEntity();

		#endregion

		#region Methods

		protected internal override async Task FilterOutInvalidModelsAsync(IList<ClientModel> models, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await base.FilterOutInvalidModelsAsync(models, result);

			var copies = models.ToArray();
			models.Clear();

			foreach(var client in copies)
			{
				var context = new ClientConfigurationValidationContext(client);
				await this.ClientValidator.ValidateAsync(context);

				if(!context.IsValid)
				{
					await this.AddErrorAsync($"Client \"{client.ClientId}\": {context.ErrorMessage}", result);
					continue;
				}

				models.Add(client);
			}
		}

		protected internal virtual IQueryable<ClientEntity> IncludeRelatedData(IQueryable<ClientEntity> query)
		{
			return query
				.Include(entity => entity.AllowedCorsOrigins)
				.Include(entity => entity.AllowedGrantTypes)
				.Include(entity => entity.AllowedScopes)
				.Include(entity => entity.Claims)
				.Include(entity => entity.ClientSecrets)
				.Include(entity => entity.IdentityProviderRestrictions)
				.Include(entity => entity.PostLogoutRedirectUris)
				.Include(entity => entity.Properties)
				.Include(entity => entity.RedirectUris);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.Clients.CountAsync());

			item.Relations.Add(typeof(ClientCorsOrigin), await this.CreateResultItemAsync(await this.DatabaseContext.ClientCorsOrigins.CountAsync()));
			item.Relations.Add(typeof(ClientClaim), await this.CreateResultItemAsync(await this.DatabaseContext.ClientClaims().CountAsync()));
			item.Relations.Add(typeof(ClientGrantType), await this.CreateResultItemAsync(await this.DatabaseContext.ClientGrantTypes().CountAsync()));
			item.Relations.Add(typeof(ClientIdPRestriction), await this.CreateResultItemAsync(await this.DatabaseContext.ClientIdentityProviderRestrictions().CountAsync()));
			item.Relations.Add(typeof(ClientPostLogoutRedirectUri), await this.CreateResultItemAsync(await this.DatabaseContext.ClientPostLogoutRedirectUris().CountAsync()));
			item.Relations.Add(typeof(ClientProperty), await this.CreateResultItemAsync(await this.DatabaseContext.ClientProperties().CountAsync()));
			item.Relations.Add(typeof(ClientRedirectUri), await this.CreateResultItemAsync(await this.DatabaseContext.ClientRedirectUris().CountAsync()));
			item.Relations.Add(typeof(ClientScope), await this.CreateResultItemAsync(await this.DatabaseContext.ClientScopes().CountAsync()));
			item.Relations.Add(typeof(ClientSecret), await this.CreateResultItemAsync(await this.DatabaseContext.ClientSecrets().CountAsync()));

			result.Items.Add(typeof(ClientEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedClientIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] {typeof(ClientEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is ClientEntity client)
						deletedClientIds.Add(client.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(ClientEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[]
				{
					typeof(ClientCorsOrigin),
					typeof(ClientClaim),
					typeof(ClientGrantType),
					typeof(ClientIdPRestriction),
					typeof(ClientPostLogoutRedirectUri),
					typeof(ClientProperty),
					typeof(ClientRedirectUri),
					typeof(ClientScope),
					typeof(ClientSecret)
				}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<ClientCorsOrigin>(await this.DatabaseContext.ClientCorsOrigins.Where(clientCorsOrigin => deletedClientIds.Contains(clientCorsOrigin.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientClaim>(await this.DatabaseContext.ClientClaims().Where(clientClaim => deletedClientIds.Contains(clientClaim.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientGrantType>(await this.DatabaseContext.ClientGrantTypes().Where(clientGrantType => deletedClientIds.Contains(clientGrantType.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientIdPRestriction>(await this.DatabaseContext.ClientIdentityProviderRestrictions().Where(clientIdentityProviderRestriction => deletedClientIds.Contains(clientIdentityProviderRestriction.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientPostLogoutRedirectUri>(await this.DatabaseContext.ClientPostLogoutRedirectUris().Where(clientPostLogoutRedirectUri => deletedClientIds.Contains(clientPostLogoutRedirectUri.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientProperty>(await this.DatabaseContext.ClientProperties().Where(clientProperty => deletedClientIds.Contains(clientProperty.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientRedirectUri>(await this.DatabaseContext.ClientRedirectUris().Where(clientRedirectUri => deletedClientIds.Contains(clientRedirectUri.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientScope>(await this.DatabaseContext.ClientScopes().Where(clientScope => deletedClientIds.Contains(clientScope.ClientId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ClientSecret>(await this.DatabaseContext.ClientSecrets().Where(clientSecret => deletedClientIds.Contains(clientSecret.ClientId)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<ClientEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.ClientId));
		}

		protected internal override IQueryable<ClientEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.ClientId)));
		}

		protected internal virtual async Task UpdateRelatedAllowedCorsOriginsAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Origin,
				(entityItem, importItem) =>
				{
					entityItem.Origin = importItem.Origin;
				},
				entity => entity.AllowedCorsOrigins,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedAllowedGrantTypesAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.GrantType,
				(entityItem, importItem) =>
				{
					entityItem.GrantType = importItem.GrantType;
				},
				entity => entity.AllowedGrantTypes,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedAllowedScopesAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Scope,
				(entityItem, importItem) =>
				{
					entityItem.Scope = importItem.Scope;
				},
				entity => entity.AllowedScopes,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedClaimsAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Type,
				(entityItem, importItem) =>
				{
					entityItem.Type = importItem.Type;
					entityItem.Value = importItem.Value;
				},
				entity => entity.Claims,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedClientSecretsAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				(entityRelations, importRelationItemToMatch) =>
				{
					// We can add more conditions here if necessary.
					return entityRelations.FirstOrDefault(entityRelation => string.Equals(entityRelation.Value, importRelationItemToMatch.Value, StringComparison.Ordinal));
				},
				(entityItem, importItem) =>
				{
					const StringComparison stringComparison = StringComparison.Ordinal;

					if(
						!string.Equals(entityItem.Description, importItem.Description, stringComparison) ||
						Nullable.Compare(entityItem.Expiration, importItem.Expiration) != 0 ||
						!string.Equals(entityItem.Type, importItem.Type, stringComparison) ||
						!string.Equals(entityItem.Value, importItem.Value, stringComparison)
					)
						entityItem.Created = importItem.Created;

					entityItem.Description = importItem.Description;
					entityItem.Expiration = importItem.Expiration;
					entityItem.Type = importItem.Type;
					entityItem.Value = importItem.Value;
				},
				entity => entity.ClientSecrets,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedIdentityProviderRestrictionsAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Provider,
				(entityItem, importItem) =>
				{
					entityItem.Provider = importItem.Provider;
				},
				entity => entity.IdentityProviderRestrictions,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedPostLogoutRedirectUrisAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.PostLogoutRedirectUri,
				(entityItem, importItem) =>
				{
					entityItem.PostLogoutRedirectUri = importItem.PostLogoutRedirectUri;
				},
				entity => entity.PostLogoutRedirectUris,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedPropertiesAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Key,
				(entityItem, importItem) =>
				{
					entityItem.Key = importItem.Key;
					entityItem.Value = importItem.Value;
				},
				entity => entity.Properties,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedRedirectUrisAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.RedirectUri,
				(entityItem, importItem) =>
				{
					entityItem.RedirectUri = importItem.RedirectUri;
				},
				entity => entity.RedirectUris,
				updates
			);
		}

		protected internal override async Task UpdateRelationsAsync(IDictionary<ClientEntity, ClientEntity> updates)
		{
			await this.UpdateRelatedAllowedCorsOriginsAsync(updates);
			await this.UpdateRelatedAllowedGrantTypesAsync(updates);
			await this.UpdateRelatedAllowedScopesAsync(updates);
			await this.UpdateRelatedClaimsAsync(updates);
			await this.UpdateRelatedClientSecretsAsync(updates);
			await this.UpdateRelatedIdentityProviderRestrictionsAsync(updates);
			await this.UpdateRelatedPostLogoutRedirectUrisAsync(updates);
			await this.UpdateRelatedPropertiesAsync(updates);
			await this.UpdateRelatedRedirectUrisAsync(updates);
		}

		#endregion
	}
}