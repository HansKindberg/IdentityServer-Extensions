using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Security.Claims.Configuration;
using HansKindberg.IdentityServer.Web;
using HansKindberg.IdentityServer.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.Security.Claims
{
	/// <inheritdoc />
	public class ClaimsSelectionContextAccessor : IClaimsSelectionContextAccessor
	{
		#region Fields

		private static readonly string _httpContextItemKey = typeof(ClaimsSelectionContext).FullName;

		#endregion

		#region Constructors

		public ClaimsSelectionContextAccessor(IAuthenticationSchemeRetriever authenticationSchemeRetriever, IOptionsMonitor<ClaimsSelectionOptions> claimsSelectionOptionsMonitor, IClaimsSelectorLoader claimsSelectorLoader, IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
		{
			this.AuthenticationSchemeRetriever = authenticationSchemeRetriever ?? throw new ArgumentNullException(nameof(authenticationSchemeRetriever));
			this.ClaimsSelectionOptionsMonitor = claimsSelectionOptionsMonitor ?? throw new ArgumentNullException(nameof(claimsSelectionOptionsMonitor));
			this.ClaimsSelectorLoader = claimsSelectorLoader ?? throw new ArgumentNullException(nameof(claimsSelectorLoader));
			this.FeatureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IAuthenticationSchemeRetriever AuthenticationSchemeRetriever { get; }
		public virtual IClaimsSelectionContext ClaimsSelectionContext => this.GetAsync(this.HttpContextAccessor.HttpContext).Result;
		protected internal virtual IOptionsMonitor<ClaimsSelectionOptions> ClaimsSelectionOptionsMonitor { get; }
		protected internal virtual IClaimsSelectorLoader ClaimsSelectorLoader { get; }
		protected internal virtual IFeatureManager FeatureManager { get; }
		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual string HttpContextItemKey => _httpContextItemKey;
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		protected internal virtual async Task<Uri> CreateClaimsSelectionUrlAsync(HttpContext httpContext, string returnUrl)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if(string.IsNullOrEmpty(returnUrl))
				returnUrl = this.ClaimsSelectionOptionsMonitor.CurrentValue.DefaultReturnUrl;

			var queryBuilder = new QueryBuilder
			{
				{ QueryStringKeys.ReturnUrl, returnUrl }
			};

			return await Task.FromResult(new Uri($"{this.ClaimsSelectionOptionsMonitor.CurrentValue.Path}{queryBuilder}", UriKind.Relative));
		}

		protected internal virtual async Task<IClaimsSelectionContext> GetAsync(HttpContext httpContext)
		{
			var httpContextItemValue = httpContext?.Items[this.HttpContextItemKey];

			if(httpContextItemValue != null)
				return httpContextItemValue as IClaimsSelectionContext;

			var claimsSelectionContext = await this.GetAsync(() => this.GetAuthenticationSchemeAsync(httpContext).Result, httpContext, () => this.GetReturnUrlAsync(httpContext).Result);

			if(httpContext != null)
				httpContext.Items[this.HttpContextItemKey] = claimsSelectionContext ?? new object();

			return claimsSelectionContext;
		}

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IClaimsSelectionContext> GetAsync(string returnUrl)
		{
			if(returnUrl == null)
				throw new ArgumentNullException(nameof(returnUrl));

			var httpContext = this.HttpContextAccessor.HttpContext;

			return await this.GetAsync(() => this.GetAuthenticationSchemeAsync(httpContext).Result, httpContext, () => returnUrl);
		}

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IClaimsSelectionContext> GetAsync(string authenticationScheme, string returnUrl)
		{
			if(authenticationScheme == null)
				throw new ArgumentNullException(nameof(authenticationScheme));

			if(returnUrl == null)
				throw new ArgumentNullException(nameof(returnUrl));

			return await this.GetAsync(() => authenticationScheme, this.HttpContextAccessor.HttpContext, () => returnUrl);
		}

		protected internal virtual async Task<IClaimsSelectionContext> GetAsync(Func<string> authenticationSchemeFunction, HttpContext httpContext, Func<string> returnUrlFunction)
		{
			if(authenticationSchemeFunction == null)
				throw new ArgumentNullException(nameof(authenticationSchemeFunction));

			if(returnUrlFunction == null)
				throw new ArgumentNullException(nameof(returnUrlFunction));

			if(!this.FeatureManager.IsEnabled(Feature.ClaimsSelection))
			{
				this.Logger.LogDebugIfEnabled($"The claims-selection-context is null because the claims-selection-feature ({typeof(Feature)}.{nameof(Feature.ClaimsSelection)}) is not enabled.");

				return null;
			}

			if(httpContext == null)
			{
				this.Logger.LogDebugIfEnabled("The claims-selection-context is null because the current http-context is null.");

				return null;
			}

			var authenticationScheme = authenticationSchemeFunction();

			if(authenticationScheme == null)
				return null;

			var claimsSelectors = (await this.ClaimsSelectorLoader.GetClaimsSelectorsAsync(authenticationScheme)).ToArray();

			if(!claimsSelectors.Any())
			{
				this.Logger.LogDebugIfEnabled($"The claims-selection-context is null because there are no claims-selectors for authentication-scheme \"{authenticationScheme}\".");

				return null;
			}

			var claimsSelectionContext = new ClaimsSelectionContext(this.ClaimsSelectionOptionsMonitor)
			{
				AuthenticationScheme = authenticationScheme,
				Url = await this.CreateClaimsSelectionUrlAsync(httpContext, returnUrlFunction())
			};

			foreach(var claimsSelector in claimsSelectors)
			{
				claimsSelectionContext.Selectors.Add(claimsSelector);
			}

			return claimsSelectionContext;
		}

		protected internal virtual async Task<string> GetAuthenticationSchemeAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			if(!httpContext.User.IsAuthenticated())
			{
				this.Logger.LogDebugIfEnabled("Could not get authentication-scheme from http-context-user-claims because the http-context-user is not authenticated.");

				return null;
			}

			var authenticationSchemeName = httpContext.User.Claims.FindFirstIdentityProviderClaim()?.Value;

			if(authenticationSchemeName == null)
			{
				this.Logger.LogWarningIfEnabled("Could not get authentication-scheme from http-context-user-claims because no suitable claim was found.");

				return null;
			}

			var authenticationScheme = await this.AuthenticationSchemeRetriever.GetAsync(authenticationSchemeName);

			if(authenticationScheme == null)
			{
				this.Logger.LogWarningIfEnabled($"The authentication-scheme \"{authenticationSchemeName}\" does not exist.");

				return null;
			}

			// ReSharper disable InvertIf
			if(!authenticationScheme.Enabled)
			{
				this.Logger.LogWarningIfEnabled($"The authentication-scheme \"{authenticationSchemeName}\" is not enabled.");

				return null;
			}
			// ReSharper restore InvertIf

			return authenticationSchemeName;
		}

		protected internal virtual async Task<string> GetReturnUrlAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var returnUrl = httpContext.Request.Path.StartsWithSegments(this.ClaimsSelectionOptionsMonitor.CurrentValue.Path, StringComparison.OrdinalIgnoreCase)
				? (string)httpContext.Request.Query[QueryStringKeys.ReturnUrl]
				: $"{httpContext.Request.Path}{httpContext.Request.QueryString}";

			return await Task.FromResult(returnUrl);
		}

		#endregion
	}
}