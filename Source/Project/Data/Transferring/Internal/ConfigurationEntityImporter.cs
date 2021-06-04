using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable StaticMemberInGenericType
	public abstract class ConfigurationEntityImporter<TEntity, TModel> : EntityImporter<TEntity, TModel> where TEntity : class
	{
		#region Fields

		private static readonly ISet<string> _propertiesToExcludeFromUpdate = new[] {"Created", "Id", "LastAccessed", "Updated"}.ToHashSet(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructors

		protected ConfigurationEntityImporter(IConfigurationDbContext databaseContext, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
		}

		#endregion

		#region Properties

		protected internal virtual IConfigurationDbContext DatabaseContext { get; }
		protected internal override ISet<string> PropertiesToExcludeFromUpdate => _propertiesToExcludeFromUpdate;

		#endregion
	}
	// ReSharper restore StaticMemberInGenericType
}