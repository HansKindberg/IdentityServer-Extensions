using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Configuration;

namespace HansKindberg.IdentityServer.Web.Authentication
{
	/// <inheritdoc />
	public class AuthenticationSchemeRetriever : IAuthenticationSchemeRetriever
	{
		#region Constructors

		public AuthenticationSchemeRetriever(IOptionsMonitor<ExtendedAuthenticationOptions> authenticationOptionsMonitor, IAuthenticationSchemeLoader authenticationSchemeLoader, IHttpContextAccessor httpContextAccessor, IIdentityProviderStore identityProviderStore, IOptionsMonitor<IdentityServerOptions> identityServerOptionsMonitor, ILoggerFactory loggerFactory, IMutualTlsService mutualTlsService)
		{
			this.AuthenticationOptionsMonitor = authenticationOptionsMonitor ?? throw new ArgumentNullException(nameof(authenticationOptionsMonitor));
			this.AuthenticationSchemeLoader = authenticationSchemeLoader ?? throw new ArgumentNullException(nameof(authenticationSchemeLoader));
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.IdentityProviderStore = identityProviderStore ?? throw new ArgumentNullException(nameof(identityProviderStore));
			this.IdentityServerOptionsMonitor = identityServerOptionsMonitor ?? throw new ArgumentNullException(nameof(identityServerOptionsMonitor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.MutualTlsService = mutualTlsService ?? throw new ArgumentNullException(nameof(mutualTlsService));
		}

		#endregion

		#region Properties

		protected internal virtual IOptionsMonitor<ExtendedAuthenticationOptions> AuthenticationOptionsMonitor { get; }
		protected internal virtual IAuthenticationSchemeLoader AuthenticationSchemeLoader { get; }
		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IIdentityProviderStore IdentityProviderStore { get; }
		protected internal virtual IOptionsMonitor<IdentityServerOptions> IdentityServerOptionsMonitor { get; }
		protected internal virtual ILogger Logger { get; }
		protected internal virtual IMutualTlsService MutualTlsService { get; }

		#endregion

		#region Methods

		public virtual async Task<IAuthenticationScheme> GetAsync(string name)
		{
			var authenticationSchemeRegistrations = this.AuthenticationOptionsMonitor.CurrentValue.SchemeRegistrations;

			if(authenticationSchemeRegistrations.ContainsKey(name))
				return await this.AuthenticationSchemeLoader.GetAsync(name);

			var identityProvider = await this.IdentityProviderStore.GetBySchemeAsync(name);

			var authenticationScheme = identityProvider != null ? new IdentityProviderWrapper(this.IdentityServerOptionsMonitor.CurrentValue.DynamicProviders, identityProvider) : null;

			if(authenticationScheme != null && authenticationScheme.Kind != AuthenticationSchemeKind.Certificate && this.IsMtlsDomainRequest())
				authenticationScheme = null;

			return authenticationScheme;
		}

		protected internal virtual bool IsMtlsDomainRequest()
		{
			var httpRequest = this.HttpContextAccessor.HttpContext?.Request;

			return httpRequest != null && this.MutualTlsService.IsMtlsDomainRequestAsync(httpRequest).Result;
		}

		public virtual async Task<IEnumerable<IAuthenticationScheme>> ListAsync()
		{
			var authenticationSchemes = (await this.AuthenticationSchemeLoader.ListAsync()).ToList();

			if(this.IsMtlsDomainRequest())
			{
				authenticationSchemes = authenticationSchemes.Where(authenticationScheme => authenticationScheme.Kind == AuthenticationSchemeKind.Certificate).ToList();
			}
			else
			{
				var dynamicProviderOptions = this.IdentityServerOptionsMonitor.CurrentValue.DynamicProviders;

				foreach(var identityProviderName in await this.IdentityProviderStore.GetAllSchemeNamesAsync())
				{
					var identityProvider = await this.IdentityProviderStore.GetBySchemeAsync(identityProviderName.Scheme);

					if(identityProvider != null && identityProvider.Enabled)
						authenticationSchemes.Add(new IdentityProviderWrapper(dynamicProviderOptions, identityProvider));
				}
			}

			return authenticationSchemes;
		}

		#endregion
	}
}