using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HansKindberg.IdentityServer.Data.Transferring.Internal
{
	// ReSharper disable StaticMemberInGenericType
	public abstract class EntityImporter<TEntity, TModel> : PartialImporter<TModel> where TEntity : class
	{
		#region Fields

		private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _propertiesCache = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

		#endregion

		#region Constructors

		protected EntityImporter(ILoggerFactory loggerFactory) : base(loggerFactory) { }

		#endregion

		#region Properties

		protected internal abstract DbSet<TEntity> Entities { get; }
		protected internal abstract Func<TEntity, string> EntityIdentifierSelector { get; }
		protected internal abstract Func<TModel, TEntity> ModelToEntityFunction { get; }
		protected internal virtual ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertiesCache => _propertiesCache;
		protected internal abstract ISet<string> PropertiesToExcludeFromUpdate { get; }

		#endregion

		#region Methods

		protected internal virtual async Task AddAsync(IEnumerable<TEntity> adds)
		{
			await this.Entities.AddRangeAsync(adds);
		}

		protected internal virtual async Task DeleteAsync(IEnumerable<TEntity> deletes)
		{
			await Task.CompletedTask;

			this.Entities.RemoveRange(deletes);
		}

		protected internal virtual Func<IList<T>, T, T> EntityMatchByStringPropertyFunction<T>(Func<T, string> equalityComparePropertyFunction)
		{
			if(equalityComparePropertyFunction == null)
				throw new ArgumentNullException(nameof(equalityComparePropertyFunction));

			return (entityRelations, importRelationItemToMatch) =>
			{
				foreach(var comparison in new[] {StringComparison.Ordinal, StringComparison.OrdinalIgnoreCase})
				{
					var matchedEntityRelationItem = entityRelations.FirstOrDefault(item => string.Equals(equalityComparePropertyFunction(item), equalityComparePropertyFunction(importRelationItemToMatch), comparison));

					if(matchedEntityRelationItem != null)
						return matchedEntityRelationItem;
				}

				return default;
			};
		}

		protected internal virtual async Task<IEnumerable<KeyValuePair<T, T>>> GetOrderedRelationsAsync<T>(Func<IList<T>, T, T> entityRelationItemMatcher, IList<T> entityRelations, IList<T> importRelations)
		{
			if(entityRelationItemMatcher == null)
				throw new ArgumentNullException(nameof(entityRelationItemMatcher));

			if(entityRelations == null)
				throw new ArgumentNullException(nameof(entityRelations));

			if(entityRelations.Any(entityItem => entityItem == null))
				throw new ArgumentException("Entity-relations can not contain null-values.", nameof(entityRelations));

			if(importRelations == null)
				throw new ArgumentNullException(nameof(importRelations));

			if(importRelations.Any(importItem => importItem == null))
				throw new ArgumentException("Import-relations can not contain null-values.", nameof(importRelations));

			var entityRelationsCopy = entityRelations.ToList();
			var importRelationsCopy = importRelations.ToList();

			var orderedRelations = new List<KeyValuePair<T, T>>();

			foreach(var importRelation in importRelations)
			{
				var entityRelation = entityRelationItemMatcher(entityRelationsCopy, importRelation);

				if(entityRelation == null)
					continue;

				orderedRelations.Add(new KeyValuePair<T, T>(entityRelation, importRelation));
				entityRelationsCopy.Remove(entityRelation);
				importRelationsCopy.Remove(importRelation);
			}

			var importRelationsSecondCopy = importRelationsCopy.ToList();

			// ReSharper disable All

			foreach(var importRelation in importRelationsCopy)
			{
				if(!entityRelationsCopy.Any())
					break;

				orderedRelations.Add(new KeyValuePair<T, T>(entityRelationsCopy[0], importRelation));
				entityRelationsCopy.RemoveAt(0);
				importRelationsSecondCopy.Remove(importRelation);
			}

			foreach(var importRelation in importRelationsSecondCopy)
			{
				orderedRelations.Add(new KeyValuePair<T, T>(default, importRelation));
			}

			foreach(var entityRelation in entityRelationsCopy)
			{
				orderedRelations.Add(new KeyValuePair<T, T>(entityRelation, default));
			}

			// ReSharper restore All

			return await Task.FromResult(orderedRelations.ToArray());
		}

		protected internal virtual async Task<IEnumerable<PropertyInfo>> GetPropertiesAsync(Type type)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));

			await Task.CompletedTask;

			return this.PropertiesCache.GetOrAdd(type, key => key.GetProperties());
		}

		protected internal override async Task ImportAsync(IList<TModel> models, ImportOptions options, IDataImportResult result)
		{
			if(models == null)
				throw new ArgumentNullException(nameof(models));

			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(result == null)
				throw new ArgumentNullException(nameof(result));

			await this.InitializeResultAsync(result);

			await this.FilterOutInvalidModelsAsync(models, result);
			await this.FilterOutDuplicateModelsAsync(models, result);

			var comparer = StringComparer.OrdinalIgnoreCase;

			var imports = models.Select(model => this.ModelToEntityFunction(model)).ToArray();
			var importIdentifiers = imports.Select(entity => this.EntityIdentifierSelector(entity)).ToHashSet(comparer);

			var updates = new Dictionary<TEntity, TEntity>();

			foreach(var entity in this.SelectUpdates(importIdentifiers))
			{
				updates.Add(entity, imports.First(import => string.Equals(this.EntityIdentifierSelector(entity), this.EntityIdentifierSelector(import), StringComparison.OrdinalIgnoreCase)));
			}

			var updateIdentifiers = updates.Keys.Select(this.EntityIdentifierSelector).ToHashSet(comparer);

			var adds = imports.Where(entity => !updateIdentifiers.Contains(this.EntityIdentifierSelector(entity)));

			await this.UpdateAsync(updates);

			await this.AddAsync(adds);

			if(options.DeleteAllOthers)
				await this.DeleteAsync(this.SelectDeletes(importIdentifiers));

			await this.PopulateResultAsync(result);
		}

		protected internal abstract IQueryable<TEntity> SelectDeletes(ISet<string> importIdentifiers);
		protected internal abstract IQueryable<TEntity> SelectUpdates(ISet<string> importIdentifiers);

		protected internal virtual async Task UpdateAsync(IDictionary<TEntity, TEntity> updates)
		{
			await this.UpdatePropertiesAsync(updates);
			await this.UpdateRelationsAsync(updates);
		}

		protected internal virtual async Task UpdatePropertiesAsync(IDictionary<TEntity, TEntity> updates)
		{
			if(updates == null)
				throw new ArgumentNullException(nameof(updates));

			if(!updates.Any())
				return;

			var properties = (await this.GetPropertiesAsync(typeof(TEntity))).Where(property => !this.PropertiesToExcludeFromUpdate.Contains(property.Name) && !typeof(IList).IsAssignableFrom(property.PropertyType)).ToArray();

			foreach(var (entity, import) in updates)
			{
				foreach(var property in properties)
				{
					property.SetValue(entity, property.GetValue(import));
				}
			}
		}

		protected internal abstract Task UpdateRelationsAsync(IDictionary<TEntity, TEntity> updates);

		protected internal virtual async Task UpdateRelationsAsync<T>(Func<IList<T>, T, T> entityRelationItemMatcher, Action<T, T> itemUpdateAction, Func<TEntity, IList<T>> listFunction, IDictionary<TEntity, TEntity> updates)
		{
			if(entityRelationItemMatcher == null)
				throw new ArgumentNullException(nameof(entityRelationItemMatcher));

			if(itemUpdateAction == null)
				throw new ArgumentNullException(nameof(itemUpdateAction));

			if(listFunction == null)
				throw new ArgumentNullException(nameof(listFunction));

			if(updates == null)
				throw new ArgumentNullException(nameof(updates));

			foreach(var (entity, import) in updates)
			{
				var entityRelations = listFunction(entity);

				foreach(var relation in await this.GetOrderedRelationsAsync(entityRelationItemMatcher, entityRelations, listFunction(import)))
				{
					if(relation.Value == null)
						entityRelations.Remove(relation.Key);
					else if(relation.Key == null)
						entityRelations.Add(relation.Value);
					else
						itemUpdateAction(relation.Key, relation.Value);
				}
			}
		}

		protected internal virtual async Task UpdateRelationsAsync<T>(Func<T, string> equalityComparePropertyFunction, Action<T, T> itemUpdateAction, Func<TEntity, IList<T>> listFunction, IDictionary<TEntity, TEntity> updates)
		{
			await this.UpdateRelationsAsync(this.EntityMatchByStringPropertyFunction(equalityComparePropertyFunction), itemUpdateAction, listFunction, updates);
		}

		#endregion
	}
	// ReSharper restore StaticMemberInGenericType
}