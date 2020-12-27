using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace HansKindberg.IdentityServer.Web.Mvc.Extensions
{
	public static class UrlHelperExtension
	{
		#region Methods

		public static Uri RemoveQuery(this IUrlHelper urlHelper, string key)
		{
			if(urlHelper == null)
				throw new ArgumentNullException(nameof(urlHelper));

			var request = urlHelper.ActionContext.HttpContext.Request;
			var query = QueryHelpers.ParseQuery(request.QueryString.ToString());
			query.Remove(key);

			var queryBuilder = new QueryBuilder();

			foreach(var (queryKey, value) in query)
			{
				queryBuilder.Add(queryKey, value.ToArray());
			}

			return new Uri(request.Path + queryBuilder.ToString(), UriKind.Relative);
		}

		#endregion
	}
}