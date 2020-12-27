using System;
using Microsoft.AspNetCore.Mvc;

namespace HansKindberg.IdentityServer.Web.Mvc.Extensions
{
	public static class ActionContextExtension
	{
		#region Methods

		public static void EnsureResponseHeader(this ActionContext context, string key, string value)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(key == null)
				throw new ArgumentNullException(nameof(key));

			if(context.HttpContext.Response.Headers.ContainsKey(key))
				return;

			context.HttpContext.Response.Headers.Add(key, value);
		}

		#endregion
	}
}