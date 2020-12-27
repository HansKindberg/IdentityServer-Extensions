using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace HansKindberg.IdentityServer.Web.Http.Extensions
{
	public static class QueryCollectionExtension
	{
		#region Methods

		public static ISet<string> GetDistinctSpaceSeparatedValues(this IQueryCollection queryCollection, string key)
		{
			return queryCollection.GetDistinctValues(key, ' ');
		}

		public static ISet<string> GetDistinctValues(this IQueryCollection queryCollection, string key, char separator)
		{
			if(queryCollection == null)
				throw new ArgumentNullException(nameof(queryCollection));

			var comparer = StringComparer.OrdinalIgnoreCase;

			// ReSharper disable InvertIf
			if(!string.IsNullOrEmpty(key))
			{
				if(!queryCollection.TryGetValue(key, out var values) || !values.Any())
				{
					var url = queryCollection.GetValueAsAbsoluteUrl("ReturnUrl");

					if(url != null)
					{
						var query = QueryHelpers.ParseQuery(url.Query);

						query.TryGetValue(key, out values);
					}
				}

				if(values.Any())
				{
					// Seems like duplicates already are removed but what the heck...
					return new HashSet<string>(values.SelectMany(culture => culture.Split(separator, StringSplitOptions.RemoveEmptyEntries)), comparer);
				}
			}
			// ReSharper restore InvertIf

			return new HashSet<string>(comparer);
		}

		public static Uri GetValueAsAbsoluteUrl(this IQueryCollection queryCollection, string key)
		{
			if(queryCollection == null)
				throw new ArgumentNullException(nameof(queryCollection));

			// ReSharper disable InvertIf
			if(!string.IsNullOrEmpty(key) && queryCollection.TryGetValue(key, out var values) && values.Any())
			{
				var value = values.First();

				if(Uri.TryCreate(value, UriKind.Absolute, out var url) || Uri.TryCreate("https://localhost" + value, UriKind.Absolute, out url))
					return url;
			}
			// ReSharper restore InvertIf

			return null;
		}

		#endregion
	}
}