using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Extensions;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IdentityResourceEntity = IdentityServer4.EntityFramework.Entities.IdentityResource;
using IdentityResourceModel = IdentityServer4.Models.IdentityResource;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class IdentityResourceImporter : ConfigurationEntityImporter<IdentityResourceEntity, IdentityResourceModel>
	{
		#region Constructors

		public IdentityResourceImporter(IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(databaseContext, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override DbSet<IdentityResourceEntity> Entities => this.DatabaseContext.IdentityResources;
		protected internal override Func<IdentityResourceEntity, string> EntityIdentifierSelector => entity => entity.Name;
		protected internal override string ModelIdentifierName => "Name";
		protected internal override Func<IdentityResourceModel, string> ModelIdentifierSelector => model => model.Name;
		protected internal override Func<IdentityResourceModel, IdentityResourceEntity> ModelToEntityFunction => model => model.ToEntity();

		#endregion

		#region Methods

		protected internal virtual IQueryable<IdentityResourceEntity> IncludeRelatedData(IQueryable<IdentityResourceEntity> query)
		{
			return query
				.Include(entity => entity.Properties)
				.Include(entity => entity.UserClaims);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.IdentityResources.CountAsync());

			item.Relations.Add(typeof(IdentityResourceClaim), await this.CreateResultItemAsync(await this.DatabaseContext.IdentityResourceClaims().CountAsync()));
			item.Relations.Add(typeof(IdentityResourceProperty), await this.CreateResultItemAsync(await this.DatabaseContext.IdentityResourceProperties().CountAsync()));

			result.Items.Add(typeof(IdentityResourceEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedIdentityResourceIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] {typeof(IdentityResourceEntity)}.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is IdentityResourceEntity identityResource)
						deletedIdentityResourceIds.Add(identityResource.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(IdentityResourceEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[] {typeof(IdentityResourceClaim), typeof(IdentityResourceProperty)}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<IdentityResourceClaim>(await this.DatabaseContext.IdentityResourceClaims().Where(identityResourceClaim => deletedIdentityResourceIds.Contains(identityResourceClaim.IdentityResourceId)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<IdentityResourceProperty>(await this.DatabaseContext.IdentityResourceProperties().Where(identityResourceProperty => deletedIdentityResourceIds.Contains(identityResourceProperty.IdentityResourceId)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<IdentityResourceEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.Name));
		}

		protected internal override IQueryable<IdentityResourceEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.Name)));
		}

		protected internal virtual async Task UpdateRelatedPropertiesAsync(IDictionary<IdentityResourceEntity, IdentityResourceEntity> updates)
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

		protected internal virtual async Task UpdateRelatedUserClaimsAsync(IDictionary<IdentityResourceEntity, IdentityResourceEntity> updates)
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

		protected internal override async Task UpdateRelationsAsync(IDictionary<IdentityResourceEntity, IdentityResourceEntity> updates)
		{
			await this.UpdateRelatedPropertiesAsync(updates);
			await this.UpdateRelatedUserClaimsAsync(updates);
		}

		#endregion
	}
}