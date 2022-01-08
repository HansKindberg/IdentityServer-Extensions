using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using HansKindberg.IdentityServer;
using HansKindberg.IdentityServer.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HttpContextFactory = UnitTests.Helpers.HttpContextFactory;

namespace UnitTests
{
	[TestClass]
	public class UriFactoryTest
	{
		#region Methods

		[TestMethod]
		public async Task CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_IfTheUriFactoryQueryModeParameterIsAll_Test()
		{
			var culture = CultureInfo.GetCultureInfo("it");

			var expectedValue = "/a/b/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=it";
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/a/b/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=it";
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/?Key-1=value-1&Key-2=value-2&key-3=value-3&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en&UI_Locales=it";
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/?Key-1=value-1&Key-2=value-2&key-3=value-3&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3FUI_Locales%3Dit";
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=it&y=z";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=it&y=z";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.All);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_IfTheUriFactoryQueryModeParameterIsNone_Test()
		{
			var culture = CultureInfo.GetCultureInfo("de");

			var expectedValue = "/a/b/?UI_Locales=de";
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.None);
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.None);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.None);
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.None);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.None);
			expectedValue = "?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: false, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.None);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.None);
			expectedValue = "?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: false, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.None);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_IfTheUriFactoryQueryModeParameterIsUiLocales_Test()
		{
			var culture = CultureInfo.GetCultureInfo("de");

			var expectedValue = "/a/b/?UI_Locales=de";
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.UiLocales);
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.UiLocales);
			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.UiLocales);
			expectedValue = "?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: false, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: true, uiLocalesInReturnUrl: false);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, true, uriFactory, UriFactoryQueryMode.UiLocales);
			expectedValue = "?UI_Locales=de";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: false, uiLocalesInReturnUrl: true);
			await this.CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(culture, expectedValue, false, uriFactory, UriFactoryQueryMode.UiLocales);
		}

		protected internal virtual async Task CreateRelativeAsync_WithCultureAndIncludeContextPathAndUriFactoryQueryModeParameters_Test(CultureInfo culture, string expectedValue, bool includeContextPath, UriFactory uriFactory, UriFactoryQueryMode uriFactoryQueryMode)
		{
			if(uriFactory == null)
				throw new ArgumentNullException(nameof(uriFactory));

			var actualValue = (await uriFactory.CreateRelativeAsync(culture, includeContextPath, uriFactoryQueryMode)).ToString();

			Assert.AreEqual(expectedValue, actualValue);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsAnEmptyString_Test()
		{
			var path = string.Empty;

			var uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: true);

			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/", path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/", path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A=B", path, "?A=B", uriFactory);

			uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: false);

			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test(string.Empty, path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test(string.Empty, path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A=B", path, "?A=B", uriFactory);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsNull_Test()
		{
			const string path = null;

			var uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: true);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/", path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/", path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A=B", path, "?A=B", uriFactory);

			uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: false);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test(string.Empty, path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test(string.Empty, path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A=B", path, "?A=B", uriFactory);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsWhitespaces_Test()
		{
			var path = "   ";

			var uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: true);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   /", path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   /", path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   /?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   /?A=B", path, "?A=B", uriFactory);

			uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: false);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   ", path, null, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   ", path, string.Empty, uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   ?A", path, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/   ?A=B", path, "?A=B", uriFactory);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfTheQueryParameterIsNotNullAndNotEmptyAndNotStartsWithAQuestionMark_ShouldAddALeadingQuestionMark()
		{
			var uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: true);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/? ", null, " ", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A", null, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A=B", null, "A=B", uriFactory);

			uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: false);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("? ", null, " ", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A", null, "A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A=B", null, "A=B", uriFactory);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfTheQueryParameterStartsWithAQuestionMark_ShouldNotAddALeadingQuestionMark()
		{
			var uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: true);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?", null, "?", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A", null, "?A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("/?A=B", null, "?A=B", uriFactory);

			uriFactory = await this.CreateUriFactoryAsync(trailingPathSlash: false);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?", null, "?", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A", null, "?A", uriFactory);
			await this.CreateRelativeAsync_WithPathAndQueryParameters_Test("?A=B", null, "?A=B", uriFactory);
		}

		protected internal virtual async Task CreateRelativeAsync_WithPathAndQueryParameters_Test(string expectedValue, string path, string query, UriFactory uriFactory)
		{
			if(uriFactory == null)
				throw new ArgumentNullException(nameof(uriFactory));

			var actualValue = (await uriFactory.CreateRelativeAsync(path, query)).ToString();

			Assert.AreEqual(expectedValue, actualValue);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_IfTheHttpContextIsNullAndTheUriFactoryQueryModeParameterIsAll_Test()
		{
			var segments = new[] { "a", "b", "c", "d" };

			var expectedValue = "/a/b/c/d/";
			var uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/a/b/c/d";
			uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.All);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_IfTheHttpContextIsNullAndTheUriFactoryQueryModeParameterIsNone_Test()
		{
			var segments = new[] { "a", "b", "c", "d" };

			var expectedValue = "/a/b/c/d/";
			var uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.None);

			expectedValue = "/a/b/c/d";
			uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.None);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_IfTheHttpContextIsNullAndTheUriFactoryQueryModeParameterIsUiLocales_Test()
		{
			var segments = new[] { "a", "b", "c", "d" };

			var expectedValue = "/a/b/c/d/";
			var uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d";
			uriFactory = await this.CreateUriFactoryAsync((HttpContext)null, trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_IfTheUriFactoryQueryModeParameterIsAll_Test()
		{
			var segments = new[] { "a", "b", "c", "d" };

			var expectedValue = "/a/b/c/d/?Key-1=value-1&Key-2=value-2&key-3=value-3&key-3=value-3";
			var uriFactory = await this.CreateUriFactoryAsync(path: null, query: "?Key-2=value-2&key-3=value-3&Key-1=value-1&Key-3=value-3", trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.All);

			expectedValue = "/a/b/c/d?Key-1=value-1&Key-2=value-2&key-3=value-3&key-3=value-3";
			uriFactory = await this.CreateUriFactoryAsync(path: null, query: "?Key-2=value-2&key-3=value-3&Key-1=value-1&Key-3=value-3", trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.All);
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_IfTheUriFactoryQueryModeParameterIsUiLocales_Test()
		{
			var segments = new[] { "a", "b", "c", "d" };

			var expectedValue = "/a/b/c/d/";
			var uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1", trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d/?UI_Locales=sv%20fr%20en";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d?UI_Locales=sv%20fr%20en";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en", trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d/?UI_Locales=fi";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: true);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);

			expectedValue = "/a/b/c/d?UI_Locales=fi";
			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1", trailingPathSlash: false);
			await this.CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(expectedValue, segments, uriFactory, UriFactoryQueryMode.UiLocales);
		}

		protected internal virtual async Task CreateRelativeAsync_WithSegmentsAndUriFactoryQueryModeParameters_Test(string expectedValue, IEnumerable<string> segments, UriFactory uriFactory, UriFactoryQueryMode uriFactoryQueryMode)
		{
			if(uriFactory == null)
				throw new ArgumentNullException(nameof(uriFactory));

			var actualValue = (await uriFactory.CreateRelativeAsync(segments, uriFactoryQueryMode)).ToString();

			Assert.AreEqual(expectedValue, actualValue);
		}

		protected internal virtual async Task<UriFactory> CreateUriFactoryAsync(string path = null, string query = null, bool trailingPathSlash = true, bool uiLocalesInReturnUrl = true)
		{
			PathString? pathString = null;

			if(path != null)
				pathString = new PathString(path);

			QueryString? queryString = null;

			if(query != null)
				queryString = new QueryString(query);

			var httpContext = await HttpContextFactory.CreateAsync(pathString, queryString);

			return await this.CreateUriFactoryAsync(httpContext, trailingPathSlash, uiLocalesInReturnUrl);
		}

		protected internal virtual async Task<UriFactory> CreateUriFactoryAsync(HttpContext httpContext, bool trailingPathSlash = true, bool uiLocalesInReturnUrl = true)
		{
			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);
			var httpContextAccessor = httpContextAccessorMock.Object;

			var logger = Mock.Of<ILogger>();

			var loggerFactoryMock = new Mock<ILoggerFactory>();
			loggerFactoryMock.Setup(loggerFactory => loggerFactory.CreateLogger(It.IsAny<string>())).Returns(logger);
			var loggerFactory = loggerFactoryMock.Object;

			var options = new UriFactoryOptions
			{
				TrailingPathSlash = trailingPathSlash,
				UiLocalesInReturnUrl = uiLocalesInReturnUrl
			};

			var optionsMonitorMock = new Mock<IOptionsMonitor<UriFactoryOptions>>();
			optionsMonitorMock.Setup(optionsMonitor => optionsMonitor.CurrentValue).Returns(options);
			var optionsMonitor = optionsMonitorMock.Object;

			return await Task.FromResult(new UriFactory(httpContextAccessor, loggerFactory, optionsMonitor));
		}

		#endregion
	}
}