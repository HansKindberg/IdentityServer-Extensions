using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.EntityFramework.Mappers;
using HansKindberg.IdentityServer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IdentityProviderEntity = Duende.IdentityServer.EntityFramework.Entities.IdentityProvider;
using IdentityProviderModel = Duende.IdentityServer.Models.IdentityProvider;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable All
	public class IdentityProviderImporter : ConfigurationEntityImporter<IdentityProviderEntity, IdentityProviderModel>
	{
		#region Constructors

		public IdentityProviderImporter(IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(databaseContext, loggerFactory) { }

		#endregion

		#region Properties

		protected internal override DbSet<IdentityProviderEntity> Entities => this.DatabaseContext.IdentityProviders;
		protected internal override Func<IdentityProviderEntity, string> EntityIdentifierSelector => entity => entity.Scheme;
		protected internal override string ModelIdentifierName => nameof(IdentityProviderModel.Scheme);
		protected internal override Func<IdentityProviderModel, string> ModelIdentifierSelector => model => model.Scheme;
		protected internal override Func<IdentityProviderModel, IdentityProviderEntity> ModelToEntityFunction => model => model.ToEntity();

		#endregion

		#region Methods

		protected internal override async Task<IList<IdentityProviderModel>> GetModelsAsync(IConfiguration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			await Task.CompletedTask;

			var models = new List<Models.IdentityProvider>();

			var configurationSection = configuration.GetSection(this.ConfigurationKey);

			configurationSection.Bind(models);

			return models.OfType<IdentityProviderModel>().ToList();
		}

		protected internal override async Task InitializeResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var item = await this.CreateResultItemAsync(await this.DatabaseContext.IdentityProviders.CountAsync());

			result.Items.Add(typeof(IdentityProviderEntity), item);
		}

		protected internal override async Task PopulateResultAsync(IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var deletedIdentityProviderIds = new HashSet<int>();

			await this.PopulateResultAsync(
				this.DatabaseContext.ChangeTracker(),
				result.Items,
				new[] { typeof(IdentityProviderEntity) }.ToHashSet(),
				entry =>
				{
					if(entry.State == EntityState.Deleted && entry.Entity is IdentityProviderEntity identityProvider)
						deletedIdentityProviderIds.Add(identityProvider.Id);

					return false;
				}
			);
		}

		protected internal override IQueryable<IdentityProviderEntity> SelectDeletes(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => !importIdentifiers.Contains(entity.Scheme));
		}

		protected internal override IQueryable<IdentityProviderEntity> SelectUpdates(ISet<string> importIdentifiers)
		{
			if(importIdentifiers == null)
				throw new ArgumentNullException(nameof(importIdentifiers));

			return this.Entities.Where(entity => importIdentifiers.Contains(entity.Scheme));
		}

		protected internal override async Task UpdateRelationsAsync(IDictionary<IdentityProviderEntity, IdentityProviderEntity> updates)
		{
			await Task.CompletedTask;
		}

		#endregion
	}
	// ReSharper restore All
}