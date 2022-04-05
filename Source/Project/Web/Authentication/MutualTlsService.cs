using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Web.Authentication
{
	/// <inheritdoc />
	public class MutualTlsService : IMutualTlsService
	{
		#region Constructors

		public MutualTlsService(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ExtendedIdentityServerOptions> identityServer, IIssuerNameService issuerNameService, ILoggerFactory loggerFactory)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.IdentityServer = identityServer ?? throw new ArgumentNullException(nameof(identityServer));
			this.IssuerNameService = issuerNameService ?? throw new ArgumentNullException(nameof(issuerNameService));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServer { get; }
		protected internal virtual IIssuerNameService IssuerNameService { get; }
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		public virtual async Task<string> GetIssuerOriginAsync()
		{
			this.Validate();

			return await this.GetIssuerOriginInternalAsync();
		}

		protected internal virtual async Task<string> GetIssuerOriginInternalAsync()
		{
			return await this.IssuerNameService.GetCurrentAsync();
		}

		public virtual async Task<string> GetMtlsOriginAsync()
		{
			return await this.GetMtlsOriginInternalAsync(this.GetValidatedOptions());
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		protected internal virtual async Task<string> GetMtlsOriginInternalAsync(ExtendedIdentityServerOptions identityServer)
		{
			if(identityServer == null)
				throw new ArgumentNullException(nameof(identityServer));

			var origin = await this.GetIssuerOriginInternalAsync();
			var mtlsOrigin = origin;

			var mtlsDomain = identityServer.MutualTls.Enabled ? identityServer.MutualTls.DomainName : null;

			// ReSharper disable InvertIf
			if(!string.IsNullOrWhiteSpace(mtlsDomain))
			{
				if(identityServer.LowerCaseIssuerUri)
					mtlsDomain = mtlsDomain.ToLowerInvariant();

				var uri = new Uri(origin);
				var authority = uri.Authority;

				mtlsOrigin = $"{uri.Scheme}{Uri.SchemeDelimiter}{mtlsDomain}.{authority}";
			}
			// ReSharper restore InvertIf

			return mtlsOrigin;
		}

		protected internal virtual ExtendedIdentityServerOptions GetValidatedOptions()
		{
			var identityServer = this.IdentityServer.CurrentValue;

			this.Validate(identityServer);

			return identityServer;
		}

		public virtual async Task<bool> IsMtlsDomainRequestAsync(HttpRequest httpRequest)
		{
			var identityServer = this.GetValidatedOptions();

			if(!identityServer.MutualTls.Enabled || string.IsNullOrWhiteSpace(identityServer.MutualTls.DomainName) || httpRequest == null)
				return false;

			var mtlsOrigin = await this.GetMtlsOriginInternalAsync(identityServer);

			var httpRequestOrigin = httpRequest.Origin();

			return string.Equals(mtlsOrigin, httpRequestOrigin, StringComparison.OrdinalIgnoreCase);
		}

		protected internal virtual void Validate()
		{
			this.Validate(this.IdentityServer.CurrentValue);
		}

		protected internal virtual void Validate(ExtendedIdentityServerOptions identityServer)
		{
			var mutualTls = identityServer?.MutualTls;

			// ReSharper disable MergeIntoNegatedPattern
			if(mutualTls == null || !mutualTls.Enabled || string.IsNullOrWhiteSpace(mutualTls.DomainName))
				return;
			// ReSharper restore MergeIntoNegatedPattern

			if(!mutualTls.DomainName.Contains('.', StringComparison.OrdinalIgnoreCase))
				return;

			var message = $"The MutualTlsOptions.DomainName can not contain dots. The current value is \"{mutualTls.DomainName}\". The reason is that the interactive mtls functionality, interactive client-certificate authentication, only supports subdomain.";

			this.Logger.LogErrorIfEnabled(message);

			throw new InvalidOperationException(message);
		}

		#endregion
	}
}