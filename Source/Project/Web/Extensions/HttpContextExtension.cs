using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace HansKindberg.IdentityServer.Web.Extensions
{
	public static class HttpContextExtension
	{
		#region Fields

		private const string _signedOutKey = "SignedOut:26cf6611-7edf-4817-9590-2888d9684303";

		#endregion

		#region Methods

		public static void SetSignedOut(this HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			httpContext.Items[_signedOutKey] = true;
		}

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		public static bool SignedOut(this HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if(httpContext.Items.TryGetValue(_signedOutKey, out var signedOutValue))
				return signedOutValue as bool? ?? false;

			return false;
		}

		#endregion
	}
}