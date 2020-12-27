using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class ConfigurationImporter : ContextImporter
	{
		#region Constructors

		public ConfigurationImporter(IClientConfigurationValidator clientValidator, IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.ClientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal virtual IClientConfigurationValidator ClientValidator { get; }
		protected internal virtual IConfigurationDbContext DatabaseContext { get; }

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
				new ApiResourceImporter(this.DatabaseContext, this.LoggerFactory),
				new ApiScopeImporter(this.DatabaseContext, this.LoggerFactory),
				new ClientImporter(this.ClientValidator, this.DatabaseContext, this.LoggerFactory),
				new IdentityResourceImporter(this.DatabaseContext, this.LoggerFactory)
			};

			return await Task.FromResult(importers.ToArray());
		}

		#endregion
	}
}