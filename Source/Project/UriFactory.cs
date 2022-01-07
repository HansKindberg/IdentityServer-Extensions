using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Web;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer
{
	public class UriFactory : IUriFactory
	{
		#region Constructors

		public UriFactory(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual ILogger Logger { get; }
		public virtual bool TrailingPathSlash { get; set; } = true;

		#endregion

		#region Methods

		protected internal virtual async Task<string> CreateQueryStringAsync(CultureInfo culture, UriFactoryQueryMode uriFactoryQueryMode)
		{
			var query = await this.ParseContextQueryAsync(uriFactoryQueryMode);

			query.Remove(QueryStringKeys.UiLocales);

			if(culture != null && !culture.Equals(CultureInfo.InvariantCulture))
				query[QueryStringKeys.UiLocales] = culture.Name;

			return await Task.FromResult(new QueryBuilder(query).ToString());
		}

		public virtual async Task<Uri> CreateRelativeAsync(string path, string query)
		{
			path ??= string.Empty;

			path = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);

			if(path.Length > 0 && !path.StartsWith(Path.AltDirectorySeparatorChar))
				path = $"{Path.AltDirectorySeparatorChar}{path}";

			if(this.TrailingPathSlash && !path.EndsWith(Path.AltDirectorySeparatorChar))
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

			var queryBuilder = new QueryBuilder(await this.ParseContextQueryAsync(uriFactoryQueryMode));

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

		protected internal virtual async Task<IDictionary<string, string>> ParseContextQueryAsync(UriFactoryQueryMode uriFactoryQueryMode)
		{
			IDictionary<string, string> empty = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
					return new Dictionary<string, string>
					{
						{ QueryStringKeys.UiLocales, string.Join(' ', uiLocales) }
					};

				return empty;
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(httpContext.Request.Query.ToSortedDictionary());
		}

		#endregion
	}
}