using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Data.Transferring.Extensions;
using HansKindberg.IdentityServer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable All
	public abstract class PartialImporter<TModel> : IPartialImporter
	{
		#region Constructors

		protected PartialImporter(ILoggerFactory loggerFactory)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual bool AllowEmptyModelIdentifier => false;
		protected internal virtual bool AllowNullModelIdentifier => false;
		protected internal virtual bool AllowWhitespacesOnlyModelIdentifier => false;
		protected internal virtual string ConfigurationKey => $"{typeof(TModel).Name}s";
		protected internal virtual ILogger Logger { get; }
		protected internal virtual string ModelIdentifierFullName => $"{typeof(TModel).Name}.{this.ModelIdentifierName}";
		protected internal abstract string ModelIdentifierName { get; }
		protected internal abstract Func<TModel, string> ModelIdentifierSelector { get; }

		#endregion

		#region Methods

		protected internal virtual async Task AddErrorAsync(string message, IDataImportResult result)
		{
			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await Task.CompletedTask;

			result.Errors.Add(message);

			this.Logger.LogErrorIfEnabled(message);
		}

		protected internal virtual async Task<IDataImportResultItem> CreateResultItemAsync(int numberOfRecords)
		{
			var resultItem = new DataImportResultItem();

			resultItem.After = resultItem.Before = numberOfRecords;

			return await Task.FromResult(resultItem);
		}

		protected internal virtual async Task FilterOutDuplicateModelsAsync(IList<TModel> models, IDataImportResult result)
		{
			await this.FilterOutDuplicateModelsAsync(models, this.ModelIdentifierFullName, this.ModelIdentifierSelector, result);
		}

		protected internal virtual async Task FilterOutDuplicateModelsAsync(IList<TModel> models, string propertyName, Func<TModel, string> propertySelector, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			if(propertySelector == null)
				throw new ArgumentNullException(nameof(propertySelector));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var errorFormat = $"{propertyName} {{0}} has {{1}} duplicate{{2}}.";

			var copies = models.ToArray();
			models.Clear();

			foreach(var group in copies.GroupBy(propertySelector, StringComparer.OrdinalIgnoreCase))
			{
				if(group.Count() > 1)
					await this.AddErrorAsync(string.Format(null, errorFormat, group.Key.ToStringRepresentation(), group.Count() - 1, group.Count() > 2 ? "s" : null), result);
				else
					models.Add(group.First());
			}
		}

		protected internal virtual async Task FilterOutInvalidModelsAsync(IList<TModel> models, IDataImportResult result)
		{
			await this.FilterOutInvalidModelsAsync(models, this.ModelIdentifierFullName, this.ModelIdentifierSelector, result, this.AllowNullModelIdentifier, this.AllowEmptyModelIdentifier, this.AllowWhitespacesOnlyModelIdentifier);
		}

		protected internal virtual async Task FilterOutInvalidModelsAsync(IList<TModel> models, string propertyName, Func<TModel, string> propertySelector, IDataImportResult result, bool allowNull = false, bool allowEmpty = false, bool allowWhitespacesOnly = false)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			if(propertySelector == null)
				throw new ArgumentNullException(nameof(propertySelector));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			var errorFormat = $"{propertyName} {{0}} can not be {{1}}.";

			var copies = models.ToArray();
			models.Clear();

			foreach(var model in copies)
			{
				var propertyValue = propertySelector(model);

				if(propertyValue == null)
				{
					if(!allowNull)
					{
						await this.AddErrorAsync($"{this.ModelIdentifierFullName} can not be null.", result);
						continue;
					}
				}
				else if(propertyValue.Length == 0)
				{
					if(!allowEmpty)
					{
						await this.AddErrorAsync(string.Format(null, errorFormat, propertyValue.ToStringRepresentation(), "empty"), result);
						continue;
					}
				}
				else if(propertyValue.Trim().Length == 0)
				{
					if(!allowWhitespacesOnly)
					{
						await this.AddErrorAsync(string.Format(null, errorFormat, propertyValue.ToStringRepresentation(), "whitespaces only"), result);
						continue;
					}
				}

				models.Add(model);
			}
		}

		protected internal virtual async Task<IList<TModel>> GetModelsAsync(IConfiguration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			await Task.CompletedTask;

			var models = new List<TModel>();

			var configurationSection = configuration.GetSection(this.ConfigurationKey);

			configurationSection.Bind(models);

			return models;
		}

		protected internal abstract Task ImportAsync(IList<TModel> models, ImportOptions options, IDataImportResult result);

		public virtual async Task ImportAsync(IConfiguration configuration, ImportOptions options, IDataImportResult result)
		{
			var models = await this.GetModelsAsync(configuration);

			await this.ImportAsync(models, options, result);
		}

		protected internal abstract Task InitializeResultAsync(IDataImportResult result);

		protected internal virtual async Task PopulateRelationDeletesAsync<T>(int numberOfDeletedRelations, IDictionary<Type, IDataImportResultItem> relations)
		{
			if(relations == null)
				throw new ArgumentNullException(nameof(relations));

			await Task.CompletedTask;

			var item = relations[typeof(T)];
			item.After -= numberOfDeletedRelations;
			item.Deletes += numberOfDeletedRelations;
		}

		protected internal abstract Task PopulateResultAsync(IDataImportResult result);

		[SuppressMessage("Style", "IDE0010:Add missing cases")]
		protected internal virtual async Task PopulateResultAsync(ChangeTracker changeTracker, IDictionary<Type, IDataImportResultItem> items, ISet<Type> types, Func<EntityEntry, bool> replacementFunction = null)
		{
			if(changeTracker == null)
				throw new ArgumentNullException(nameof(changeTracker));

			if(items == null)
				throw new ArgumentNullException(nameof(items));

			if(types == null)
				throw new ArgumentNullException(nameof(types));

			replacementFunction ??= entry => false;

			await Task.CompletedTask;

			foreach(var entry in changeTracker.Entries())
			{
				var type = entry.Entity.GetType();

				if(!types.Contains(type))
					continue;

				var state = entry.State;

				switch(state)
				{
					case EntityState.Added:
					{
						if(!replacementFunction(entry))
						{
							items.IncrementAdds(type);
							items.IncrementAfter(type);
						}

						break;
					}
					case EntityState.Deleted:
					{
						if(!replacementFunction(entry))
						{
							items.DecrementAfter(type);
							items.IncrementDeletes(type);
						}

						break;
					}
					case EntityState.Modified:
					{
						if(!replacementFunction(entry))
							items.IncrementUpdates(type);

						break;
					}
					case EntityState.Unchanged:
					{
						replacementFunction(entry);
						break;
					}
					default:
					{
						throw new NotSupportedException($"Entity-state {state} is not supported.");
					}
				}
			}
		}

		#endregion
	}
	// ReSharper restore All
}