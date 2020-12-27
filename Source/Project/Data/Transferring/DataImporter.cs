using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Transferring.Internal;
using HansKindberg.IdentityServer.Identity;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Data.Transferring
{
	/// <inheritdoc />
	public class DataImporter : IDataImporter
	{
		#region Fields

		private IList<IContextImporter> _importers;

		#endregion

		#region Constructors

		public DataImporter(IClientConfigurationValidator clientValidator, IConfigurationDbContext configurationDatabaseContext, IIdentityFacade identityFacade, ILoggerFactory loggerFactory)
		{
			this.ClientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
			this.ConfigurationDatabaseContext = configurationDatabaseContext ?? throw new ArgumentNullException(nameof(configurationDatabaseContext));
			this.IdentityFacade = identityFacade ?? throw new ArgumentNullException(nameof(identityFacade));
			this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.Logger = loggerFactory.CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IClientConfigurationValidator ClientValidator { get; }
		protected internal virtual IConfigurationDbContext ConfigurationDatabaseContext { get; }
		protected internal virtual IIdentityFacade IdentityFacade { get; }
		protected internal virtual IList<IContextImporter> Importers => this._importers ??= new List<IContextImporter>(this.CreateImportersAsync().Result);
		protected internal virtual ILogger Logger { get; }
		protected internal virtual ILoggerFactory LoggerFactory { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<IEnumerable<IContextImporter>> CreateImportersAsync()
		{
			var importers = new List<IContextImporter>
			{
				new ConfigurationImporter(this.ClientValidator, this.ConfigurationDatabaseContext, this.LoggerFactory),
				new IdentityImporter(this.IdentityFacade, this.LoggerFactory)
			};

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