using System;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace HansKindberg.IdentityServer.Web.Localization
{
	/// <inheritdoc />
	public class OpenIdConnectRequestCultureProvider : RequestCultureProvider
	{
		#region Properties

		public virtual string QueryStringKey { get; } = QueryStringKeys.UiLocales;

		#endregion

		#region Methods

		public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var uiLocales = httpContext.Request.Query.GetDistinctSpaceSeparatedValues(this.QueryStringKey);

			if(uiLocales.Any())
				return new ProviderCultureResult(uiLocales.Select(culture => new StringSegment(culture)).ToList());

			return await NullProviderCultureResult;
		}

		#endregion
	}
}