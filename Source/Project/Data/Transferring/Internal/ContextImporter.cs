using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public abstract class ContextImporter : IContextImporter
	{
		#region Fields

		private IList<IPartialImporter> _importers;

		#endregion

		#region Constructors

		protected ContextImporter(ILoggerFactory loggerFactory)
		{
			this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.Logger = loggerFactory.CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IList<IPartialImporter> Importers => this._importers ??= new List<IPartialImporter>(this.CreateImportersAsync().Result);
		protected internal virtual ILogger Logger { get; }
		protected internal virtual ILoggerFactory LoggerFactory { get; }

		#endregion

		#region Methods

		public abstract Task<int> CommitAsync();
		protected internal abstract Task<IEnumerable<IPartialImporter>> CreateImportersAsync();

		public virtual async Task ImportAsync(IConfiguration configuration, ImportOptions options, IDataImportResult result)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			foreach(var importer in this.Importers)
			{
				await importer.ImportAsync(configuration, options, result);
			}
		}

		#endregion
	}
}