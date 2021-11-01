using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.WsFederation.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rsk.WsFederation.EntityFramework.DbContexts;
using Rsk.WsFederation.EntityFramework.Entities;
using Rsk.WsFederation.EntityFramework.Mappers;
using RelyingPartyEntity = Rsk.WsFederation.EntityFramework.Entities.RelyingParty;
using RelyingPartyModel = Rsk.WsFederation.Models.RelyingParty;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal.WsFederation
{
	// ReSharper disable_ All
	public class RelyingPartyImporter : EntityImporter<RelyingPartyEntity, RelyingPartyModel>
	{
		#region Fields

		private static readonly ISet<string> _propertiesToExcludeFromUpdate = new[] { "Id" }.ToHashSet(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		public RelyingPartyImporter(IWsFederationConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal override string ConfigurationKey => nameof(IWsFederationConfigurationDbContext.RelyingParties);
		protected internal virtual IWsFederationConfigurationDbContext DatabaseContext { get; }
		protected internal override DbSet<RelyingPartyEntity> Entities => this.DatabaseContext.RelyingParties;
		protected internal override Func<RelyingPartyEntity, string> EntityIdentifierSelector => entity => entity.Realm;
		protected internal override string ModelIdentifierName => nameof(RelyingPartyModel.Realm);
		protected internal override Func<RelyingPartyModel, string> ModelIdentifierSelector => model => model.Realm;
		protected internal override Func<RelyingPartyModel, RelyingPartyEntity> ModelToEntityFunction => model => model.ToEntity();
		protected internal override ISet<string> PropertiesToExcludeFromUpdate => _propertiesToExcludeFromUpdate;

		#endregion

		#region Methods

		protected internal virtual IQueryable<RelyingPartyEntity> IncludeRelatedData(IQueryable<RelyingPartyEntity> query)
		{
			return query
				.Include(entity => entity.ClaimMapping);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.RelyingParties.CountAsync());

			item.Relations.Add(typeof(WsFederationClaimMap), await this.CreateResultItemAsync(await this.DatabaseContext.ClaimMapping().CountAsync()));

			result.Items.Add(typeof(RelyingPartyEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedRelyingPartiesIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] { typeof(RelyingPartyEntity) }.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is RelyingPartyEntity relyingParty)
						deletedRelyingPartiesIds.Add(relyingParty.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(RelyingPartyEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[]
				{
					typeof(WsFederationClaimMap)
				}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<WsFederationClaimMap>(await this.DatabaseContext.ClaimMapping().Where(wsFederationClaimMap => deletedRelyingPartiesIds.Contains(wsFederationClaimMap.RelyingParty.Id)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<RelyingPartyEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.Realm));
		}

		protected internal override IQueryable<RelyingPartyEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.Realm)));
		}

		protected internal virtual async Task UpdateRelatedClaimsMappingAsync(IDictionary<RelyingPartyEntity, RelyingPartyEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.OriginalClaimType,
				(entityItem, importItem) =>
				{
					entityItem.NewClaimType = importItem.NewClaimType;
					entityItem.OriginalClaimType = importItem.OriginalClaimType;
				},
				entity => entity.ClaimMapping,
				updates
			);
		}

		protected internal override async Task UpdateRelationsAsync(IDictionary<RelyingPartyEntity, RelyingPartyEntity> updates)
		{
			await this.UpdateRelatedClaimsMappingAsync(updates);
		}

		#endregion
	}
	// ReSharper restore All
}