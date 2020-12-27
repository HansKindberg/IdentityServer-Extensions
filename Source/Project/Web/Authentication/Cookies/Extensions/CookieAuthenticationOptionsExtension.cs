using System;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HansKindberg.IdentityServer.Web.Authentication.Cookies.Extensions
{
	public static class CookieAuthenticationOptionsExtension
	{
		#region Methods

		public static void SetDefaults(this CookieAuthenticationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			options.LoginPath = Paths.SignIn;
			options.LogoutPath = Paths.SignOut;
			options.ReturnUrlParameter = QueryStringKeys.ReturnUrl;
		}

		#endregion
	}
}