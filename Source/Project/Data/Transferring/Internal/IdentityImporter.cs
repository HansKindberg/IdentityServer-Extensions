using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Identity;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	public class IdentityImporter : ContextImporter
	{
		#region Constructors

		public IdentityImporter(IIdentityFacade facade, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
		}

		#endregion

		#region Properties

		protected internal virtual IIdentityFacade Facade { get; }

		#endregion

		#region Methods

		public override async Task<int> CommitAsync()
		{
			return await this.Facade.DatabaseContext.SaveChangesAsync();
		}

		protected internal override async Task<IEnumerable<IPartialImporter>> CreateImportersAsync()
		{
			var importers = new List<IPartialImporter>
			{
				new UserImporter(this.Facade, this.LoggerFactory)
			};

			return await Task.FromResult(importers.ToArray());
		}

		#endregion
	}
}