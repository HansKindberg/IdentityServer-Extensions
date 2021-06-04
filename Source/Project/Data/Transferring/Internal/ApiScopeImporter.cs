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
using ApiScopeEntity = Duende.IdentityServer.EntityFramework.Entities.ApiScope;
using ApiScopeModel = Duende.IdentityServer.Models.ApiScope;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class ApiScopeImporter : ConfigurationEntityImporter<ApiScopeEntity, ApiScopeModel>
	{
		#region Constructors

		public ApiScopeImporter(IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(databaseContext, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override DbSet<ApiScopeEntity> Entities => this.DatabaseContext.ApiScopes;
		protected internal override Func<ApiScopeEntity, string> EntityIdentifierSelector => entity => entity.Name;
		protected internal override string ModelIdentifierName => nameof(ApiScopeModel.Name);
		protected internal override Func<ApiScopeModel, string> ModelIdentifierSelector => model => model.Name;
		protected internal override Func<ApiScopeModel, ApiScopeEntity> ModelToEntityFunction => model => model.ToEntity();

		#endregion

		#region Methods

		protected internal virtual IQueryable<ApiScopeEntity> IncludeRelatedData(IQueryable<ApiScopeEntity> query)
		{
			return query
				.Include(entity => entity.Properties)
				.Include(entity => entity.UserClaims);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.ApiScopes.CountAsync());

			item.Relations.Add(typeof(ApiScopeClaim), await this.CreateResultItemAsync(await this.DatabaseContext.ApiScopeClaims().CountAsync()));
			item.Relations.Add(typeof(ApiScopeProperty), await this.CreateResultItemAsync(await this.DatabaseContext.ApiScopeProperties().CountAsync()));

			result.Items.Add(typeof(ApiScopeEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedApiScopeIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] {typeof(ApiScopeEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is ApiScopeEntity apiScope)
						deletedApiScopeIds.Add(apiScope.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(ApiScopeEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[] {typeof(ApiScopeClaim), typeof(ApiScopeProperty)}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<ApiScopeClaim>(await this.DatabaseContext.ApiScopeClaims().Where(apiScopeClaim => deletedApiScopeIds.Contains(apiScopeClaim.ScopeId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<ApiScopeProperty>(await this.DatabaseContext.ApiScopeProperties().Where(apiScopeProperty => deletedApiScopeIds.Contains(apiScopeProperty.ScopeId)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<ApiScopeEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.Name));
		}

		protected internal override IQueryable<ApiScopeEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.Name)));
		}

		protected internal virtual async Task UpdateRelatedPropertiesAsync(IDictionary<ApiScopeEntity, ApiScopeEntity> updates)
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

		protected internal virtual async Task UpdateRelatedUserClaimsAsync(IDictionary<ApiScopeEntity, ApiScopeEntity> updates)
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

		protected internal override async Task UpdateRelationsAsync(IDictionary<ApiScopeEntity, ApiScopeEntity> updates)
		{
			await this.UpdateRelatedPropertiesAsync(updates);
			await this.UpdateRelatedUserClaimsAsync(updates);
		}

		#endregion
	}
}