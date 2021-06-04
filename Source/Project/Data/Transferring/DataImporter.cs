using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.IdentityServer.Validation;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using HansKindberg.IdentityServer.Data.Transferring.Internal.Saml;
using HansKindberg.IdentityServer.Data.Transferring.Internal.WsFederation;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Logging.Extensions;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;
using Rsk.WsFederation.EntityFramework.DbContexts;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataImporter : IDataImporter
	{
		#region Fields

		private IList<IContextImporter> _importers;

		#endregion

		#region Constructors

		public DataImporter(IClientConfigurationValidator clientValidator, IConfigurationDbContext configurationDatabaseContext, IFeatureManager featureManager, IIdentityFacade identityFacade, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
		{
			this.ClientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
			this.ConfigurationDatabaseContext = configurationDatabaseContext ?? throw new ArgumentNullException(nameof(configurationDatabaseContext));
			this.FeatureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			this.IdentityFacade = identityFacade ?? throw new ArgumentNullException(nameof(identityFacade));
			this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.Logger = loggerFactory.CreateLogger(this.GetType());
			this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		#endregion

		#region Properties

		protected internal virtual IClientConfigurationValidator ClientValidator { get; }
		protected internal virtual IConfigurationDbContext ConfigurationDatabaseContext { get; }
		protected internal virtual IFeatureManager FeatureManager { get; }
		protected internal virtual IIdentityFacade IdentityFacade { get; }
		protected internal virtual IList<IContextImporter> Importers => this._importers ??= new List<IContextImporter>(this.CreateImportersAsync().Result);
		protected internal virtual ILogger Logger { get; }
		protected internal virtual ILoggerFactory LoggerFactory { get; }
		protected internal virtual IServiceProvider ServiceProvider { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<IEnumerable<IContextImporter>> CreateImportersAsync()
		{
			var importers = new List<IContextImporter>
			{
				new ConfigurationImporter(this.ClientValidator, this.ConfigurationDatabaseContext, this.LoggerFactory),
				new IdentityImporter(this.IdentityFacade, this.LoggerFactory)
			};

			if(this.FeatureManager.IsEnabled(Feature.Saml))
				importers.Add(new SamlConfigurationImporter(this.ServiceProvider.GetRequiredService<ISamlConfigurationDbContext>(), this.LoggerFactory));

			if(this.FeatureManager.IsEnabled(Feature.WsFederation))
				importers.Add(new WsFederationConfigurationImporter(this.ServiceProvider.GetRequiredService<IWsFederationConfigurationDbContext>(), this.LoggerFactory));

			return await Task.FromResult(importers.ToArray());
		}

		public virtual async Task<IDataImportResult> ImportAsync(IConfiguration configuration, ImportOptions options)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			var result = new DataImportResult();

			foreach(var importer in this.Importers)
			{
				await importer.ImportAsync(configuration, options, result);
			}

			if(!result.Errors.Any() && !options.VerifyOnly)
			{
				foreach(var importer in this.Importers)
				{
					result.SavedChanges += await importer.CommitAsync();
				}
			}

			this.Logger.LogDebugIfEnabled($"Data-import finished: errors = {result.Errors.Count}, saved changes = {result.SavedChanges}, verify only = {options.VerifyOnly}");

			return result;
		}

		#endregion
	}
}