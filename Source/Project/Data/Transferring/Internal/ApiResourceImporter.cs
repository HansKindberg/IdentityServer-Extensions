using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Mappers;
using HansKindberg.IdentityServer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApiResourceEntity = Duende.IdentityServer.EntityFramework.Entities.ApiResource;
using ApiResourceModel = Duende.IdentityServer.Models.ApiResource;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class ApiResourceImporter : ConfigurationEntityImporter<ApiResourceEntity, ApiResourceModel>
	{
		#region Constructors

		public ApiResourceImporter(IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(databaseContext, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override DbSet<ApiResourceEntity> Entities => this.DatabaseContext.ApiResources;
		protected internal override Func<ApiResourceEntity, string> EntityIdentifierSelector => entity => entity.Name;
		protected internal override string ModelIdentifierName => nameof(ApiResourceModel.Name);
		protected internal override Func<ApiResourceModel, string> ModelIdentifierSelector => model => model.Name;
		protected internal override Func<ApiResourceModel, ApiResourceEntity> ModelToEntityFunction => model => model.ToEntity();

		#endregion

		#region Methods

		protected internal virtual IQueryable<ApiResourceEntity> IncludeRelatedData(IQueryable<ApiResourceEntity> query)
		{
			return query
				.Include(entity => entity.Properties)
				.Include(entity => entity.Scopes)
				.Include(entity => entity.Secrets)
				.Include(entity => entity.UserClaims);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.ApiResources.CountAsync());

			item.Relations.Add(typeof(ApiResourceClaim), await this.CreateResultItemAsync(await this.DatabaseContext.ApiResourceClaims().CountAsync()));
			item.Relations.Add(typeof(ApiResourceProperty), await this.CreateResultItemAsync(await this.DatabaseContext.ApiResourceProperties().CountAsync()));
			item.Relations.Add(typeof(ApiResourceScope), await this.CreateResultItemAsync(await this.DatabaseContext.ApiResourceScopes().CountAsync()));
			item.Relations.Add(typeof(ApiResourceSecret), await this.CreateResultItemAsync(await this.DatabaseContext.ApiResourceSecrets().CountAsync()));

			result.Items.Add(typeof(ApiResourceEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedApiResourceIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] {typeof(ApiResourceEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is ApiResourceEntity apiResource)
						deletedApiResourceIds.Add(apiResource.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(ApiResourceEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[] {typeof(ApiResourceClaim), typeof(ApiResourceProperty), typeof(ApiResourceScope), typeof(ApiResourceSecret)}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<ApiResourceClaim>(await this.DatabaseContext.ApiResourceClaims().Where(apiResourceClaim => deletedApiResourceIds.Contains(apiResourceClaim.ApiResourceId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ApiResourceProperty>(await this.DatabaseContext.ApiResourceProperties().Where(apiResourceProperty => deletedApiResourceIds.Contains(apiResourceProperty.ApiResourceId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ApiResourceScope>(await this.DatabaseContext.ApiResourceScopes().Where(apiResourceScope => deletedApiResourceIds.Contains(apiResourceScope.ApiResourceId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ApiResourceSecret>(await this.DatabaseContext.ApiResourceSecrets().Where(apiResourceSecret => deletedApiResourceIds.Contains(apiResourceSecret.ApiResourceId)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<ApiResourceEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.Name));
		}

		protected internal override IQueryable<ApiResourceEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.Name)));
		}

		protected internal virtual async Task UpdateRelatedPropertiesAsync(IDictionary<ApiResourceEntity, ApiResourceEntity> updates)
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

		protected internal virtual async Task UpdateRelatedScopesAsync(IDictionary<ApiResourceEntity, ApiResourceEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Scope,
				(entityItem, importItem) =>
				{
					entityItem.Scope = importItem.Scope;
				},
				entity => entity.Scopes,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedSecretsAsync(IDictionary<ApiResourceEntity, ApiResourceEntity> updates)
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
				entity => entity.Secrets,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedUserClaimsAsync(IDictionary<ApiResourceEntity, ApiResourceEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Type,
				(entityItem, importItem) =>
				{
					entityItem.Type = importItem.Type;
				},
				entity => entity.UserClaims,
				updates
			);
		}

		protected internal override async Task UpdateRelationsAsync(IDictionary<ApiResourceEntity, ApiResourceEntity> updates)
		{
			await this.UpdateRelatedPropertiesAsync(updates);
			await this.UpdateRelatedScopesAsync(updates);
			await this.UpdateRelatedSecretsAsync(updates);
			await this.UpdateRelatedUserClaimsAsync(updates);
		}

		#endregion
	}
}