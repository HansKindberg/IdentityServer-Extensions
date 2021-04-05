using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rsk.WsFederation.EntityFramework.DbContexts;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal.WsFederation
{
	public class WsFederationConfigurationImporter : ContextImporter
	{
		#region Constructors

		public WsFederationConfigurationImporter(IWsFederationConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal virtual IWsFederationConfigurationDbContext DatabaseContext { get; }

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
				new RelyingPartyImporter(this.DatabaseContext, this.LoggerFactory)
			};

			return await Task.FromResult(importers.ToArray());
		}

		#endregion
	}
}