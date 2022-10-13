using System;
using HansKindberg.IdentityServer.Web.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Configuration
{
	public class PostConfigureInternalCookieAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
	{
		#region Constructors

		public PostConfigureInternalCookieAuthenticationOptions(IOptionsMonitor<ExtendedIdentityServerOptions> identityServer, ILoggerFactory loggerFactory, IMutualTlsService mutualTlsService)
		{
			this.IdentityServer = identityServer ?? throw new ArgumentNullException(nameof(identityServer));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.MutualTlsService = mutualTlsService ?? throw new ArgumentNullException(nameof(mutualTlsService));
		}

		#endregion

		#region Properties

		protected internal virtual IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServer { get; }
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IMutualTlsService MutualTlsService { get; }

		#endregion

		#region Methods

		public virtual void PostConfigure(string name, CookieAuthenticationOptions options)
		{
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			if(!string.Equals(name, this.IdentityServer.CurrentValue.IntermediateCookieAuthenticationHandlers.Certificate.Name, StringComparison.OrdinalIgnoreCase))
				return;

			var issuerOrigin = this.MutualTlsService.GetIssuerOriginAsync().Result;
			var mtlsOrigin = this.MutualTlsService.GetMtlsOriginAsync().Result;

			if(string.Equals(issuerOrigin, mtlsOrigin, StringComparison.OrdinalIgnoreCase))
				return;

			var domain = new Uri(issuerOrigin).Host;

			this.Logger.LogDebugIfEnabled($"Setting cookie-domain to \"{domain}\" for cookie-authentication-options with name \"{name}\", to be able to handle interactive mTLS-authentication.");

			options.Cookie.Domain = domain;
		}

		#endregion
	}
}