using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Data.Transferring.Extensions
{
	public static class DataImportResultItemDictionaryExtension
	{
		#region Methods

		private static void Change(IDictionary<Type, IDataImportResultItem> dictionary, int numberOfItems, Action<IDataImportResultItem, int> setter, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			if(setter == null)
				throw new ArgumentNullException(nameof(setter));

			if(type == null)
				throw new ArgumentNullException(nameof(type));

			if(!dictionary.TryGetValue(type, out var item))
			{
				dictionary.Add(type, item);
				return;
			}

			setter(item, numberOfItems);
		}

		private static void Decrement(IDictionary<Type, IDataImportResultItem> dictionary, Action<IDataImportResultItem, int> setter, Type type)
		{
			Change(dictionary, -1, setter, type);
		}

		public static void DecrementAfter(this IDictionary<Type, IDataImportResultItem> dictionary, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			Decrement(dictionary, (item, difference) => { item.After += difference; }, type);
		}

		private static void Increment(IDictionary<Type, IDataImportResultItem> dictionary, Action<IDataImportResultItem, int> setter, Type type)
		{
			Change(dictionary, 1, setter, type);
		}

		public static void IncrementAdds(this IDictionary<Type, IDataImportResultItem> dictionary, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			Increment(dictionary, (item, difference) => { item.Adds += difference; }, type);
		}

		public static void IncrementAfter(this IDictionary<Type, IDataImportResultItem> dictionary, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			Increment(dictionary, (item, difference) => { item.After += difference; }, type);
		}

		public static void IncrementDeletes(this IDictionary<Type, IDataImportResultItem> dictionary, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			Increment(dictionary, (item, difference) => { item.Deletes += difference; }, type);
		}

		public static void IncrementUpdates(this IDictionary<Type, IDataImportResultItem> dictionary, Type type)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			Increment(dictionary, (item, difference) => { item.Updates += difference; }, type);
		}

		#endregion
	}
}