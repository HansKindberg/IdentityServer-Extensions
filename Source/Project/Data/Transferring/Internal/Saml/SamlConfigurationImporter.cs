using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rsk.Saml.IdentityProvider.Storage.EntityFramework.Interfaces;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal.Saml
{
	public class SamlConfigurationImporter : ContextImporter
	{
		#region Constructors

		public SamlConfigurationImporter(ISamlConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal virtual ISamlConfigurationDbContext DatabaseContext { get; }

		#endregion

		#region Methods

		public override async Task<int> CommitAsync()
		{
			return await this.DatabaseContext.SaveChangesAsync();
		}

		protected internal override async Task<IEnumerable<IPartialImporter>> CreateImportersAsync()
		{
			var importers = new List<IPartialImporter>
			{
				new ServiceProviderImporter(this.DatabaseContext, this.LoggerFactory)
			};

			return await Task.FromResult(importers.ToArray());
		}

		#endregion
	}
}