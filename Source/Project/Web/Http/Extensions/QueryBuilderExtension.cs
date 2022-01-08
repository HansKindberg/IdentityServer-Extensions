using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace HansKindberg.IdentityServer.Web.Http.Extensions
{
	public static class QueryBuilderExtension
	{
		#region Methods

		/// <summary>
		/// Only to handle netcoreapp3.1. Already supported in net5.0.
		/// </summary>
		public static QueryBuilder Create(IEnumerable<KeyValuePair<string, StringValues>> parameters)
		{
			parameters ??= Enumerable.Empty<KeyValuePair<string, StringValues>>();

#if NET5_0_OR_GREATER
			return new QueryBuilder(parameters);
#else
			// https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/QueryBuilder.cs#L42
			return new QueryBuilder(parameters.SelectMany(item => item.Value, (item, value) => KeyValuePair.Create(item.Key, value ?? string.Empty)));
#endif
		}

		#endregion
	}
}