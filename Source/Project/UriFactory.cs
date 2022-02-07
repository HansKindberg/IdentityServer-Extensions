using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Web;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer
{
	public class UriFactory : IUriFactory
	{
		#region Constructors

		public UriFactory(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IOptionsMonitor<UriFactoryOptions> optionsMonitor)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual ILogger Logger { get; }
		public virtual IOptionsMonitor<UriFactoryOptions> OptionsMonitor { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<string> CreateQueryStringAsync(CultureInfo culture, UriFactoryQueryMode uriFactoryQueryMode)
		{
			var query = await this.ParseContextQueryAsync(uriFactoryQueryMode);

			if(this.OptionsMonitor.CurrentValue.UiLocalesInReturnUrl && this.TryGetReturnUrlAsAbsoluteUrl(query, out var returnUrl))
			{
				var returnUrlQuery = QueryHelpers.ParseQuery(returnUrl.Query);

				this.ResolveCultureQuery(culture, returnUrlQuery);

				var uriBuilder = new UriBuilder(returnUrl)
				{
					Query = QueryBuilderExtension.Create(returnUrlQuery).ToString()
				};

				query[QueryStringKeys.ReturnUrl] = uriBuilder.Uri.PathAndQuery;
			}
			else
			{
				this.ResolveCultureQuery(culture, query);
			}

			return await Task.FromResult(QueryBuilderExtension.Create(query).ToString());
		}

		public virtual async Task<Uri> CreateRelativeAsync(string path, string query)
		{
			path ??= string.Empty;

			path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);

			if(path.Length > 0 && !path.StartsWith(Path.AltDirectorySeparatorChar))
				path = $"{Path.AltDirectorySeparatorChar}{path}";

			if(this.OptionsMonitor.CurrentValue.TrailingPathSlash && !path.EndsWith(Path.AltDirectorySeparatorChar))
				path = $"{path}{Path.AltDirectorySeparatorChar}";

			query ??= string.Empty;

			const char questionMark = '?';

			if(query.Length > 0 && !query.StartsWith(questionMark))
				query = $"{questionMark}{query}";

			return await Task.FromResult(new Uri(path + query, UriKind.Relative));
		}

		public virtual async Task<Uri> CreateRelativeAsync(IEnumerable<string> segments, UriFactoryQueryMode uriFactoryQueryMode = UriFactoryQueryMode.UiLocales)
		{
			var path = await this.JoinSegmentsAsync(segments);

			var queryBuilder = QueryBuilderExtension.Create(await this.ParseContextQueryAsync(uriFactoryQueryMode));

			return await this.CreateRelativeAsync(path, queryBuilder.ToString());
		}

		public virtual async Task<Uri> CreateRelativeAsync(CultureInfo culture, bool includeContextPath = true, UriFactoryQueryMode uriFactoryQueryMode = UriFactoryQueryMode.UiLocales)
		{
			string path = null;

			if(includeContextPath)
			{
				var httpContext = this.HttpContextAccessor.HttpContext;

				if(httpContext != null)
					path = httpContext.Request.Path;
				else
					this.Logger.LogWarningIfEnabled("Include-context-path is true but the http-context is null.");
			}

			var query = await this.CreateQueryStringAsync(culture, uriFactoryQueryMode);

			return await this.CreateRelativeAsync(path, query);
		}

		protected internal virtual async Task<string> JoinSegmentsAsync(IEnumerable<string> segments)
		{
			segments = (segments ?? Enumerable.Empty<string>()).ToArray();

			const char pathSeparator = '/';

			return await Task.FromResult(pathSeparator + (segments.Any() ? string.Join(pathSeparator.ToString(CultureInfo.InvariantCulture), segments) + pathSeparator : string.Empty));
		}

		protected internal virtual async Task<IDictionary<string, StringValues>> ParseContextQueryAsync(UriFactoryQueryMode uriFactoryQueryMode)
		{
			var stringComparer = StringComparer.OrdinalIgnoreCase;

			IDictionary<string, StringValues> empty = new Dictionary<string, StringValues>(stringComparer);

			if(uriFactoryQueryMode == UriFactoryQueryMode.None)
				return empty;

			var httpContext = this.HttpContextAccessor.HttpContext;

			if(httpContext == null)
			{
				this.Logger.LogWarningIfEnabled($"Uri-factory-query-mode is \"{uriFactoryQueryMode}\" but the http-context is null.");
				return empty;
			}

			// ReSharper disable InvertIf
			if(uriFactoryQueryMode == UriFactoryQueryMode.UiLocales)
			{
				var uiLocales = httpContext.Request.Query.GetUiLocales();

				if(uiLocales.Any())
					return new Dictionary<string, StringValues>(stringComparer)
					{
						// ui_locales, space-separated, https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
						{ QueryStringKeys.UiLocales, new StringValues(string.Join(' ', uiLocales)) }
					};

				return empty;
			}
			// ReSharper restore InvertIf

			var query = new SortedDictionary<string, StringValues>(stringComparer);

			foreach(var (key, value) in httpContext.Request.Query)
			{
				query.Add(key, value);
			}

			return await Task.FromResult(query);
		}

		protected internal virtual void ResolveCultureQuery(CultureInfo culture, IDictionary<string, StringValues> query)
		{
			if(query == null)
				throw new ArgumentNullException(nameof(query));

			query.Remove(QueryStringKeys.UiLocales);

			if(culture != null && !culture.Equals(CultureInfo.InvariantCulture))
				query[QueryStringKeys.UiLocales] = culture.Name;
		}

		protected internal virtual bool TryGetReturnUrlAsAbsoluteUrl(IDictionary<string, StringValues> query, out Uri url)
		{
			if(query == null)
				throw new ArgumentNullException(nameof(query));

			url = null;

			// ReSharper disable InvertIf
			if(query.TryGetValue(QueryStringKeys.ReturnUrl, out var values))
			{
				var value = values.FirstOrDefault();

				return value.TryGetAsAbsoluteUrl(out url);
			}
			// ReSharper restore InvertIf

			return false;
		}

		#endregion
	}
}