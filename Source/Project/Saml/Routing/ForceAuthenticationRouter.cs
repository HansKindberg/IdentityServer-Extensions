using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Saml.Routing.Configuration;
using HansKindberg.IdentityServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HansKindberg.IdentityServer.Saml.Routing
{
	/// <inheritdoc />
	public class ForceAuthenticationRouter : IForceAuthenticationRouter
	{
		#region Constructors

		public ForceAuthenticationRouter(IHttpContextAccessor httpContextAccessor, IOptionsMonitor<ForceAuthenticationRouterOptions> optionsMonitor)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual IOptionsMonitor<ForceAuthenticationRouterOptions> OptionsMonitor { get; }

		#endregion

		#region Methods

		[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
		public virtual async Task<IActionResult> GetActionResultAsync(string returnUrl)
		{
			var queryBuilder = new QueryBuilder();

			if(returnUrl != null)
				queryBuilder.Add(QueryStringKeys.ReturnUrl, returnUrl);

			var uiLocales = await this.GetUiLocalesAsync();

			if(uiLocales.Any())
				queryBuilder.Add(QueryStringKeys.UiLocales, uiLocales.ToArray());

			var urlBuilder = new UriBuilder("https://_")
			{
				Path = this.OptionsMonitor.CurrentValue.Path,
				Query = queryBuilder.ToString()
			};

			var redirectResult = new RedirectResult(urlBuilder.Uri.PathAndQuery);

			return await Task.FromResult(redirectResult);
		}

		protected internal virtual async Task<StringValues> GetUiLocalesAsync()
		{
			var httpContext = this.HttpContextAccessor.HttpContext;

			var queryCollection = httpContext?.Request.Query;

			if(queryCollection != null && queryCollection.TryGetValue(QueryStringKeys.UiLocales, out var values) && values.Any())
				return await Task.FromResult(values);

			return default;
		}

		#endregion
	}
}