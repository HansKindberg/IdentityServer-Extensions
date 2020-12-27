using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Web.Mvc.Extensions;
using HansKindberg.IdentityServer.Web.Mvc.Filters.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Extensions;

namespace HansKindberg.IdentityServer.Web.Mvc.Filters
{
	/// <summary>
	/// From https://github.com/IdentityServer/IdentityServer4/blob/main/src/IdentityServer4/host/Quickstart/SecurityHeadersAttribute.cs
	/// </summary>
	public class SecurityHeadersFilter : IResultFilter
	{
		#region Constructors

		public SecurityHeadersFilter(IFeatureManager featureManager, IOptions<SecurityHeaderOptions> options)
		{
			this.FeatureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}

		#endregion

		#region Properties

		protected internal virtual IFeatureManager FeatureManager { get; }
		protected internal virtual IOptions<SecurityHeaderOptions> Options { get; }

		#endregion

		#region Methods

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		protected internal virtual bool Allow(HttpContext httpContext, Func<ISet<string>> pathsFunction)
		{
			var path = (string)httpContext?.Request?.Path;

			if(path == null)
				return false;

			pathsFunction ??= () => new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			return pathsFunction().Where(item => item != null).Any(item => path.Like(item));
		}

		protected internal virtual bool AllowDataImage(HttpContext httpContext)
		{
			return this.Allow(httpContext, () => this.Options.Value.AllowDataImagePaths);
		}

		protected internal virtual bool AllowInlineImage(HttpContext httpContext)
		{
			return this.Allow(httpContext, () => this.Options.Value.AllowInlineImagePaths);
		}

		protected internal virtual bool AllowInlineScript(HttpContext httpContext)
		{
			return this.Allow(httpContext, () => this.Options.Value.AllowInlineScriptPaths);
		}

		protected internal virtual bool AllowInlineStyle(HttpContext httpContext)
		{
			return this.Allow(httpContext, () => this.Options.Value.AllowInlineStylePaths);
		}

		public virtual void OnResultExecuted(ResultExecutedContext context) { }

		public virtual void OnResultExecuting(ResultExecutingContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(!this.FeatureManager.IsEnabled(Feature.SecurityHeaders))
				return;

			if(!(context.Result is ViewResult))
				return;

			// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
			context.EnsureResponseHeader("X-Content-Type-Options", "nosniff");

			// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
			context.EnsureResponseHeader("X-Frame-Options", "SAMEORIGIN");

			// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy
			var contentSecurityPolicy = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";

			var allowDataImage = this.AllowDataImage(context.HttpContext);
			var allowInlineImage = this.AllowInlineImage(context.HttpContext);

			if(allowDataImage || allowInlineImage)
			{
				contentSecurityPolicy += "img-src 'self'";

				if(allowDataImage)
					contentSecurityPolicy += " data:";

				if(allowInlineImage)
					contentSecurityPolicy += " 'unsafe-inline'";

				contentSecurityPolicy += ";";
			}

			if(this.AllowInlineScript(context.HttpContext))
				contentSecurityPolicy += "script-src 'self' 'unsafe-inline';";

			if(this.AllowInlineStyle(context.HttpContext))
				contentSecurityPolicy += "style-src 'self' 'unsafe-inline';";

			if(context.HttpContext.Request.IsHttps)
				contentSecurityPolicy += "upgrade-insecure-requests;";

			// Also an example if you need client images to be displayed from twitter
			// contentSecurityPolicy += "img-src 'self' https://pbs.twimg.com;";

			// Standard compliant browsers
			context.EnsureResponseHeader("Content-Security-Policy", contentSecurityPolicy);

			// IE
			context.EnsureResponseHeader("X-Content-Security-Policy", contentSecurityPolicy);

			// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Referrer-Policy
			context.EnsureResponseHeader("Referrer-Policy", "no-referrer");
		}

		#endregion
	}
}