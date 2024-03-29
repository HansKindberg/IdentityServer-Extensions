using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Collections.Generic.Extensions
{
	public static class DictionaryExtension
	{
		#region Methods

		public static IDictionary<TKey, TValue> With<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			dictionary[key] = value;

			return dictionary;
		}

		public static IDictionary<TKey, TValue> Without<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			if(dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			dictionary.Remove(key);

			return dictionary;
		}

		#endregion
	}
}