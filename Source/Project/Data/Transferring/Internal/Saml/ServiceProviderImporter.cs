using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Saml.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Entities;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Mappers;
using ServiceProviderEntity = Rsk.Saml.IdentityProvider.Storage.EntityFramework.Entities.ServiceProvider;
using ServiceProviderModel = Rsk.Saml.Models.ServiceProvider;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal.Saml
{
	// ReSharper disable_ All
	public class ServiceProviderImporter : EntityImporter<ServiceProviderEntity, ServiceProviderModel>
	{
		#region Fields

		private static readonly ISet<string> _propertiesToExcludeFromUpdate = new[] { "Id" }.ToHashSet(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		public ServiceProviderImporter(ISamlConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal virtual ISamlConfigurationDbContext DatabaseContext { get; }
		protected internal override DbSet<ServiceProviderEntity> Entities => this.DatabaseContext.ServiceProviders;
		protected internal override Func<ServiceProviderEntity, string> EntityIdentifierSelector => entity => entity.EntityId;
		protected internal override string ModelIdentifierName => nameof(ServiceProviderModel.EntityId);
		protected internal override Func<ServiceProviderModel, string> ModelIdentifierSelector => model => model.EntityId;
		protected internal override Func<ServiceProviderModel, ServiceProviderEntity> ModelToEntityFunction => model => model.ToEntity();
		protected internal override ISet<string> PropertiesToExcludeFromUpdate => _propertiesToExcludeFromUpdate;

		#endregion

		#region Methods

		protected internal virtual IQueryable<ServiceProviderEntity> IncludeRelatedData(IQueryable<ServiceProviderEntity> query)
		{
			return query
				.Include(entity => entity.AssertionConsumerServices)
				.Include(entity => entity.ClaimsMapping)
				.Include(entity => entity.SigningCertificates)
				.Include(entity => entity.SingleLogoutServices);
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.ServiceProviders.CountAsync());

			item.Relations.Add(typeof(AssertionConsumerService), await this.CreateResultItemAsync(await this.DatabaseContext.AssertionConsumerServices().CountAsync()));
			item.Relations.Add(typeof(SamlClaimMap), await this.CreateResultItemAsync(await this.DatabaseContext.ClaimsMapping().CountAsync()));
			item.Relations.Add(typeof(SigningCertificate), await this.CreateResultItemAsync(await this.DatabaseContext.SigningCertificates().CountAsync()));
			item.Relations.Add(typeof(SingleLogoutService), await this.CreateResultItemAsync(await this.DatabaseContext.SingleLogoutServices().CountAsync()));

			result.Items.Add(typeof(ServiceProviderEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedServiceProviderIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] { typeof(ServiceProviderEntity) }.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is ServiceProviderEntity serviceProvider)
						deletedServiceProviderIds.Add(serviceProvider.Id);

					return false;
				}
			);

			var parent = result.Items[typeof(ServiceProviderEntity)];

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				parent.Relations,
				new[]
				{
					typeof(AssertionConsumerService),
					typeof(SamlClaimMap),
					typeof(SigningCertificate),
					typeof(SingleLogoutService)
				}.ToHashSet()
			);

			await this.PopulateRelationDeletesAsync<AssertionConsumerService>(await this.DatabaseContext.AssertionConsumerServices().Where(assertionConsumerService => deletedServiceProviderIds.Contains(assertionConsumerService.ServiceProvider.Id)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<SamlClaimMap>(await this.DatabaseContext.ClaimsMapping().Where(samlClaimMap => deletedServiceProviderIds.Contains(samlClaimMap.ServiceProvider.Id)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<SigningCertificate>(await this.DatabaseContext.SigningCertificates().Where(signingCertificate => deletedServiceProviderIds.Contains(signingCertificate.ServiceProvider.Id)).CountAsync(), parent.Relations);
			await this.PopulateRelationDeletesAsync<SingleLogoutService>(await this.DatabaseContext.SingleLogoutServices().Where(singleLogoutService => deletedServiceProviderIds.Contains(singleLogoutService.ServiceProvider.Id)).CountAsync(), parent.Relations);
		}

		protected internal override IQueryable<ServiceProviderEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.EntityId));
		}

		protected internal override IQueryable<ServiceProviderEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.IncludeRelatedData(this.Entities.Where(entity => importIdentifiers.Contains(entity.EntityId)));
		}

		protected internal virtual async Task UpdateRelatedAssertionConsumerServicesAsync(IDictionary<ServiceProviderEntity, ServiceProviderEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Binding,
				(entityItem, importItem) =>
				{
					entityItem.Binding = importItem.Binding;
					entityItem.Index = importItem.Index;
					entityItem.IsDefault = importItem.IsDefault;
					entityItem.Location = importItem.Location;
				},
				entity => entity.AssertionConsumerServices,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedClaimsMappingAsync(IDictionary<ServiceProviderEntity, ServiceProviderEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.OriginalClaimType,
				(entityItem, importItem) =>
				{
					entityItem.NewClaimType = importItem.NewClaimType;
					entityItem.OriginalClaimType = importItem.OriginalClaimType;
				},
				entity => entity.ClaimsMapping,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedSigningCertificatesAsync(IDictionary<ServiceProviderEntity, ServiceProviderEntity> updates)
		{
			await this.UpdateRelationsAsync(
				(entityRelations, importRelationItemToMatch) =>
				{
					// We can add more conditions here if necessary.
					return entityRelations.FirstOrDefault(entityRelation => string.Equals(Convert.ToBase64String(entityRelation.Certificate), Convert.ToBase64String(importRelationItemToMatch.Certificate), StringComparison.Ordinal));
				},
				(entityItem, importItem) =>
				{
					entityItem.Certificate = importItem.Certificate;
				},
				entity => entity.SigningCertificates,
				updates
			);
		}

		protected internal virtual async Task UpdateRelatedSingleLogoutServicesAsync(IDictionary<ServiceProviderEntity, ServiceProviderEntity> updates)
		{
			await this.UpdateRelationsAsync(
				entityItem => entityItem.Binding,
				(entityItem, importItem) =>
				{
					entityItem.Binding = importItem.Binding;
					entityItem.Index = importItem.Index;
					entityItem.IsDefault = importItem.IsDefault;
					entityItem.Location = importItem.Location;
				},
				entity => entity.SingleLogoutServices,
				updates
			);
		}

		protected internal override async Task UpdateRelationsAsync(IDictionary<ServiceProviderEntity, ServiceProviderEntity> updates)
		{
			await this.UpdateRelatedAssertionConsumerServicesAsync(updates);
			await this.UpdateRelatedClaimsMappingAsync(updates);
			await this.UpdateRelatedSigningCertificatesAsync(updates);
			await this.UpdateRelatedSingleLogoutServicesAsync(updates);
		}

		#endregion
	}
	// ReSharper restore All
}