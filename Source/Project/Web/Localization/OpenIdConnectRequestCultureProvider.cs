using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Services;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace HansKindberg.IdentityServer.Web.Localization
{
	/// <inheritdoc />
	public class OpenIdConnectRequestCultureProvider : RequestCultureProvider
	{
		#region Properties

		protected internal virtual string CultureKey { get; } = QueryStringKeys.UiLocales;
		protected internal virtual string SignOutKey { get; } = QueryStringKeys.SignOutId;

		#endregion

		#region Methods

		public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var uiLocales = httpContext.Request.Query.GetDistinctSpaceSeparatedValues(this.CultureKey);

			if(!uiLocales.Any())
				uiLocales = await this.GetUiLocalesBySignOutAsync(httpContext);

			if(uiLocales.Any())
				return new ProviderCultureResult(uiLocales.Select(culture => new StringSegment(culture)).ToList());

			return await NullProviderCultureResult;
		}

		protected internal virtual async Task<IIdentityServerInteractionService> GetInteractionServiceAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			return await Task.FromResult(httpContext.RequestServices.GetRequiredService<IIdentityServerInteractionService>());
		}

		protected internal virtual async Task<ISet<string>> GetUiLocalesBySignOutAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var query = new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase);

			// ReSharper disable InvertIf
			if(httpContext.Request.Query.ContainsKey(this.SignOutKey))
			{
				var interaction = await this.GetInteractionServiceAsync(httpContext);

				var signOutRequest = await interaction.GetLogoutContextAsync(httpContext.Request.Query[this.SignOutKey]);

				var uiLocales = signOutRequest?.Parameters[this.CultureKey];

				if(uiLocales != null)
					query.Add(this.CultureKey, uiLocales);
			}
			// ReSharper restore InvertIf

			return new QueryCollection(query).GetDistinctSpaceSeparatedValues(this.CultureKey);
		}

		#endregion
	}
}