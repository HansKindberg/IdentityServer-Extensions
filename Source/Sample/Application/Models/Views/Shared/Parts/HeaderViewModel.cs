using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Application.Controllers;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Localization.Extensions;
using HansKindberg.IdentityServer.Navigation;
using HansKindberg.IdentityServer.Web;
using HansKindberg.IdentityServer.Web.Authorization;
using HansKindberg.IdentityServer.Web.Extensions;
using HansKindberg.IdentityServer.Web.Http.Extensions;
using HansKindberg.IdentityServer.Web.Localization;
using HansKindberg.IdentityServer.Web.Routing;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace Application.Models.Views.Shared.Parts
{
	public class HeaderViewModel
	{
		#region Fields

		private Lazy<string> _cultureCookieValue;
		private Lazy<string> _environmentName;
		private INavigationNode _navigation;
		private Lazy<IRequestCultureFeature> _requestCultureFeature;
		private Lazy<Uri> _returnUrlAsAbsoluteUrl;
		private IEnumerable<CultureInfo> _supportedUiCultures;
		private Lazy<INavigationNode> _uiCultureNavigation;

		#endregion

		#region Constructors

		public HeaderViewModel(IFacade facade)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
			this.HttpContext = (facade.HttpContextAccessor ?? throw new ArgumentException("The http-context-accessor-property can not be null.", nameof(facade))).HttpContext;
			this.Localizer = (facade.LocalizerFactory ?? throw new ArgumentException("The localizer-factory-property can not be null.", nameof(facade))).CreateFromType(this.GetType());
		}

		#endregion

		#region Properties

		public virtual CultureInfo Culture => CultureInfo.CurrentCulture;
		public virtual string CultureCookieName => CookieRequestCultureProvider.DefaultCookieName;

		public virtual string CultureCookieValue
		{
			get
			{
				this._cultureCookieValue ??= new Lazy<string>(() => this.HttpContext.Request.Cookies.TryGetValue(this.CultureCookieName, out var value) ? value : null);

				return this._cultureCookieValue.Value;
			}
		}

		public virtual string EnvironmentName
		{
			get
			{
				this._environmentName ??= new Lazy<string>(() =>
				{
					var localizedValue = this.Localizer.GetString("environment-name");

					return localizedValue.ResourceNotFound ? null : localizedValue;
				});

				return this._environmentName.Value;
			}
		}

		protected internal virtual IFacade Facade { get; }
		protected internal virtual HttpContext HttpContext { get; }
		protected internal virtual IStringLocalizer Localizer { get; }

		public virtual INavigationNode Navigation
		{
			get
			{
				// ReSharper disable InvertIf
				if(this._navigation == null)
				{
					var navigation = new NavigationNode(null);

					if(this.HttpContext.User.IsAuthenticated() && !this.HttpContext.SignedOut())
						this.AddGrantsNavigationNode(navigation);

					if(this.Facade.FeatureManager.IsEnabled(Feature.Home))
						navigation.Url = this.GetUrl();

					if(!this.HttpContext.SignedOut() && this.Facade.AuthorizationResolver.HasPermissionAsync(Permissions.Administrator, this.HttpContext.User).Result)
					{
						if(this.Facade.FeatureManager.IsEnabled(Feature.DataTransfer))
							this.AddNavigationNode(new[] {nameof(DataTransferController.Index), nameof(DataTransferController.Export), nameof(DataTransferController.Import)}, nameof(Feature.DataTransfer), navigation);

						if(this.Facade.FeatureManager.IsEnabled(Feature.Diagnostics))
							this.AddNavigationNode(new[] {nameof(DiagnosticsController.Index), nameof(DiagnosticsController.AuthenticationScheme), nameof(DiagnosticsController.Configuration), nameof(DiagnosticsController.Cookies), nameof(DiagnosticsController.DataProtection), nameof(DiagnosticsController.EnvironmentVariables), nameof(DiagnosticsController.Options), nameof(DiagnosticsController.RequestHeaders), nameof(DiagnosticsController.ResponseHeaders)}, nameof(Feature.Diagnostics), navigation);

						if(this.Facade.FeatureManager.IsEnabled(Feature.Debug))
							this.AddDebugNavigationNode(navigation);
					}

					this._navigation = navigation;
				}
				// ReSharper restore InvertIf

				return this._navigation;
			}
		}

		protected internal virtual IRequestCultureFeature RequestCultureFeature
		{
			get
			{
				this._requestCultureFeature ??= new Lazy<IRequestCultureFeature>(() => this.HttpContext.Features.Get<IRequestCultureFeature>());

				return this._requestCultureFeature.Value;
			}
		}

		protected internal virtual Uri ReturnUrlAsAbsoluteUrl
		{
			get
			{
				this._returnUrlAsAbsoluteUrl ??= new Lazy<Uri>(() => this.HttpContext.Request.Query.GetValueAsAbsoluteUrl(QueryStringKeys.ReturnUrl));

				return this._returnUrlAsAbsoluteUrl.Value;
			}
		}

		protected internal virtual IEnumerable<CultureInfo> SupportedUiCultures => this._supportedUiCultures ??= this.Facade.RequestLocalization.CurrentValue.SupportedUICultures.OrderBy(item => item.NativeName, StringComparer.Ordinal);
		public virtual CultureInfo UiCulture => CultureInfo.CurrentUICulture;

		public virtual INavigationNode UiCultureNavigation
		{
			get
			{
				this._uiCultureNavigation ??= new Lazy<INavigationNode>(() =>
				{
					var supportedUiCultures = this.SupportedUiCultures.ToArray();

					if(supportedUiCultures.Length < 2)
						return null;

					var uiCultureNavigation = new NavigationNode(null)
					{
						Active = true,
						Text = this.UiCulture.NativeName,
						Tooltip = this.GetCultureNavigationTooltip()
					};

					if(this.HttpContext.Request.Query.TryGetValue(QueryStringKeys.UiLocales, out var values) && values.Any())
					{
						uiCultureNavigation.Children.Add(new NavigationNode(uiCultureNavigation)
						{
							Text = this.Localizer.GetString("- Clear -"),
							Tooltip = this.Localizer.GetString("Clear the selected culture."),
							Url = this.CreateRelativeUrl((CultureInfo)null)
						});
					}

					foreach(var supportedUiCulture in supportedUiCultures)
					{
						uiCultureNavigation.Children.Add(new NavigationNode(uiCultureNavigation)
						{
							Active = supportedUiCulture.Equals(this.UiCulture),
							Text = supportedUiCulture.NativeName,
							Tooltip = this.Localizer.GetString("Select culture {0}.", supportedUiCulture.Name),
							Url = this.CreateRelativeUrl(supportedUiCulture)
						});
					}

					return uiCultureNavigation;
				});

				return this._uiCultureNavigation.Value;
			}
		}

		#endregion

		#region Methods

		protected internal virtual void AddDebugNavigationNode(NavigationNode navigation)
		{
			if(navigation == null)
				throw new ArgumentNullException(nameof(navigation));

			var query = QueryHelpers.ParseQuery(this.HttpContext.Request.QueryString.ToString());
			query[QueryStringKeys.Debug] = true.ToString();

			navigation.Children.Add(new NavigationNode(navigation)
			{
				Text = this.Localizer.GetString("Debug/Heading"),
				Tooltip = this.Localizer.GetString("Debug/Information"),
				Url = this.CreateRelativeUrl(this.HttpContext.Request.Path, this.CreateQueryBuilder(query).ToString())
			});
		}

		protected internal virtual void AddGrantsNavigationNode(NavigationNode navigation)
		{
			if(navigation == null)
				throw new ArgumentNullException(nameof(navigation));

			const string name = "Grants";
			var localizationPath = $":Views.{name}.{nameof(GrantsController.Index)}.";

			navigation.Children.Add(new NavigationNode(navigation)
			{
				Active = this.StringEquals(this.HttpContext.GetRouteValue(RouteKeys.Controller) as string, name),
				Text = this.Localizer.GetString($"{localizationPath}Label"),
				Tooltip = this.Localizer.GetString($"{localizationPath}Information"),
				Url = this.GetUrl(null, name)
			});
		}

		protected internal virtual void AddNavigationNode(IEnumerable<string> children, string name, NavigationNode navigation)
		{
			if(navigation == null)
				throw new ArgumentNullException(nameof(navigation));

			var localizationPathPrefix = $":Views.{name}.";
			var localizationPath = localizationPathPrefix;

			var navigationNode = new NavigationNode(navigation)
			{
				Active = this.StringEquals(this.HttpContext.GetRouteValue(RouteKeys.Controller) as string, name),
				Text = this.Localizer.GetString($"{localizationPath}Heading"),
				Tooltip = this.Localizer.GetString($"{localizationPath}Information")
			};

			navigation.Children.Add(navigationNode);

			foreach(var actionName in children ?? Enumerable.Empty<string>())
			{
				localizationPath = $"{localizationPathPrefix}{actionName}.";

				var node = new NavigationNode(navigationNode)
				{
					Active = navigationNode.Active && this.StringEquals(this.HttpContext.GetRouteValue(RouteKeys.Action) as string, actionName),
					Text = this.Localizer.GetString($"{localizationPath}Heading"),
					Tooltip = this.Localizer.GetString($"{localizationPath}Information"),
					Url = this.GetUrl(!string.Equals(actionName, nameof(DiagnosticsController.Index), StringComparison.OrdinalIgnoreCase) ? actionName : null, name)
				};

				navigationNode.Children.Add(node);
			}
		}

		protected internal virtual QueryBuilder CreateQueryBuilder(IDictionary<string, StringValues> query)
		{
			if(query == null)
				throw new ArgumentNullException(nameof(query));

			var queryBuilder = new QueryBuilder();

			foreach(var (key, value) in query)
			{
				queryBuilder.Add(key, value.ToArray());
			}

			return queryBuilder;
		}

		[SuppressMessage("Globalization", "CA1304:Specify CultureInfo")]
		protected internal virtual QueryBuilder CreateQueryBuilder(IDictionary<string, StringValues> query, CultureInfo uiCulture)
		{
			if(query == null)
				throw new ArgumentNullException(nameof(query));

			this.ResolveCultureQuery(query, uiCulture);

			return this.CreateQueryBuilder(query);
		}

		[SuppressMessage("Globalization", "CA1304:Specify CultureInfo")]
		protected internal virtual string CreateQueryString(CultureInfo uiCulture)
		{
			var query = QueryHelpers.ParseQuery(this.HttpContext.Request.QueryString.ToString());

			if(this.ReturnUrlAsAbsoluteUrl != null)
			{
				var returnUrlQuery = QueryHelpers.ParseQuery(this.ReturnUrlAsAbsoluteUrl.Query);

				this.ResolveCultureQuery(returnUrlQuery, uiCulture);

				var uriBuilder = new UriBuilder(this.ReturnUrlAsAbsoluteUrl)
				{
					Query = this.CreateQueryBuilder(returnUrlQuery).ToString()
				};

				query[QueryStringKeys.ReturnUrl] = uriBuilder.Uri.PathAndQuery;
			}
			else
			{
				this.ResolveCultureQuery(query, uiCulture);
			}

			return this.CreateQueryBuilder(query).ToString();
		}

		protected internal virtual Uri CreateRelativeUrl(IEnumerable<string> segments)
		{
			return this.CreateRelativeUrl(this.JoinSegments(segments), this.HttpContext.Request.QueryString.ToString());
		}

		protected internal virtual Uri CreateRelativeUrl(string path, string queryString)
		{
			return new Uri(path + queryString, UriKind.Relative);
		}

		protected internal virtual Uri CreateRelativeUrl(CultureInfo uiCulture)
		{
			var queryString = this.CreateQueryString(uiCulture);

			return this.CreateRelativeUrl(this.HttpContext.Request.Path, queryString);
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		protected internal virtual string GetCultureNavigationTooltip()
		{
			var informationArgument = this.RequestCultureFeature.Provider switch
			{
				null => this.Localizer.GetString("the default settings"),
				AcceptLanguageHeaderRequestCultureProvider _ => this.Localizer.GetString("the request-header (browser-settings)"),
				CookieRequestCultureProvider _ => this.Localizer.GetString("a cookie"),
				OpenIdConnectRequestCultureProvider _ => this.Localizer.GetString("the query-string"),
				_ => null
			};

			string tooltip = this.Localizer.GetString("Select a culture.");

			if(informationArgument != null)
				tooltip += " " + this.Localizer.GetString("The current culture is determined by {0}.", informationArgument);

			return tooltip;
		}

		protected internal virtual Uri GetUrl()
		{
			return this.GetUrl(null);
		}

		public virtual Uri GetUrl(string controller)
		{
			return this.GetUrl(null, controller);
		}

		public virtual Uri GetUrl(string action, string controller)
		{
			var segments = new List<string>();

			if(!string.IsNullOrEmpty(controller) && !this.StringEquals(controller, "Home"))
				segments.Add(controller);

			if(!string.IsNullOrEmpty(action) && !this.StringEquals(action, nameof(HomeController.Index)))
				segments.Add(action);

			return this.CreateRelativeUrl(segments);
		}

		protected internal virtual string JoinSegments(IEnumerable<string> segments)
		{
			segments = (segments ?? Enumerable.Empty<string>()).ToArray();

			const char pathSeparator = '/';

			return pathSeparator + (segments.Any() ? string.Join(pathSeparator.ToString(CultureInfo.InvariantCulture), segments) + pathSeparator : string.Empty);
		}

		protected internal virtual void ResolveCultureQuery(IDictionary<string, StringValues> query, CultureInfo uiCulture)
		{
			if(query == null)
				throw new ArgumentNullException(nameof(query));

			if(uiCulture == null || uiCulture.Equals(CultureInfo.InvariantCulture))
				query.Remove(QueryStringKeys.UiLocales);
			else
				query[QueryStringKeys.UiLocales] = uiCulture.Name;
		}

		protected internal virtual bool StringEquals(string first, string second)
		{
			return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}