using System;
using Duende.IdentityServer;
using HansKindberg.IdentityServer.Web.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HansKindberg.IdentityServer.Configuration
{
	public class PostConfigureInternalCookieAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
	{
		#region Constructors

		public PostConfigureInternalCookieAuthenticationOptions(IHttpContextAccessor httpContextAccessor, IMutualTlsService mutualTlsService)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.MutualTlsService = mutualTlsService ?? throw new ArgumentNullException(nameof(mutualTlsService));
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IMutualTlsService MutualTlsService { get; }

		#endregion

		#region Methods

		public virtual void PostConfigure(string name, CookieAuthenticationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(!string.Equals(name, IdentityServerConstants.ExternalCookieAuthenticationScheme, StringComparison.OrdinalIgnoreCase))
				return;

			var httpRequest = this.HttpContextAccessor.HttpContext?.Request;

			if(httpRequest == null)
				return;

			if(!this.MutualTlsService.IsMtlsDomainRequestAsync(httpRequest).Result)
				return;

			var issuerOrigin = this.MutualTlsService.GetIssuerOriginAsync().Result;

			var domain = new Uri(issuerOrigin).Host;

			options.Cookie.Domain = domain;
		}

		#endregion
	}
}