using System.Globalization;
using System.Threading.Tasks;
using HansKindberg.IdentityServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
		public async Task CreateRelativeAsync_WithCultureAndQueryModeParameters_IfTheQueryModeParameterIsAll_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/a/b/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.All)).ToString());

			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en");
			Assert.AreEqual("/?Key-1=value-1&Key-2=value-2&key-3=value-3&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en&UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), false, UriFactoryQueryMode.All)).ToString());

			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/?Key-1=value-1&Key-2=value-2&key-3=value-3&UI_Locales=de&y=z", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.All)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithCultureAndQueryModeParameters_IfTheQueryModeParameterIsNone_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/a/b/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.None)).ToString());

			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en");
			Assert.AreEqual("/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), false, UriFactoryQueryMode.None)).ToString());

			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.None)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithCultureAndQueryModeParameters_IfTheQueryModeParameterIsUiLocales_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/a/b/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.UiLocales)).ToString());

			uriFactory = await this.CreateUriFactoryAsync("/a/b", "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en");
			Assert.AreEqual("/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), false)).ToString());

			uriFactory = await this.CreateUriFactoryAsync(null, "?y=z&Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/?UI_Locales=de", (await uriFactory.CreateRelativeAsync(CultureInfo.GetCultureInfo("de"), uriFactoryQueryMode: UriFactoryQueryMode.UiLocales)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsAnEmptyString_ShouldWork()
		{
			var path = string.Empty;

			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = true;

			var uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual("/", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual("/", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("/?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("/?A=B", uri.ToString());

			uriFactory.TrailingPathSlash = false;

			uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual(string.Empty, uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual(string.Empty, uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("?A=B", uri.ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsNull_ShouldWork()
		{
			const string path = null;

			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = true;

			var uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual("/", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual("/", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("/?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("/?A=B", uri.ToString());

			uriFactory.TrailingPathSlash = false;

			uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual(string.Empty, uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual(string.Empty, uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("?A=B", uri.ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfThePathParameterIsWhitespaces_ShouldWork()
		{
			var path = "   ";

			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = true;

			var uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual("/   /", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual("/   /", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("/   /?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("/   /?A=B", uri.ToString());

			uriFactory.TrailingPathSlash = false;

			uri = await uriFactory.CreateRelativeAsync(path, null);
			Assert.AreEqual("/   ", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, string.Empty);
			Assert.AreEqual("/   ", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "A");
			Assert.AreEqual("/   ?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(path, "?A=B");
			Assert.AreEqual("/   ?A=B", uri.ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfTheQueryParameterIsNotNullAndNotEmpty_ShouldAddAQuestionMark()
		{
			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = true;

			var uri = await uriFactory.CreateRelativeAsync(null, " ");
			Assert.AreEqual("/? ", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(null, "A");
			Assert.AreEqual("/?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(null, "A=B");
			Assert.AreEqual("/?A=B", uri.ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithPathAndQueryParameters_IfTheQueryParameterStartsWithAQuestionMark_ShouldNotAddAQuestionMark()
		{
			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = true;

			var uri = await uriFactory.CreateRelativeAsync(null, "?");
			Assert.AreEqual("/?", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(null, "?A");
			Assert.AreEqual("/?A", uri.ToString());

			uri = await uriFactory.CreateRelativeAsync(null, "?A=B");
			Assert.AreEqual("/?A=B", uri.ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndQueryModeParameters_IfTheHttpContextIsNullAndTheQueryModeParameterIsAll_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync(null);

			Assert.AreEqual("/a/b/c/d/", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.All)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndQueryModeParameters_IfTheHttpContextIsNullAndTheQueryModeParameterIsNone_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync(null);

			Assert.AreEqual("/a/b/c/d/", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.None)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndQueryModeParameters_IfTheHttpContextIsNullAndTheQueryModeParameterIsUiLocales_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync(null);

			Assert.AreEqual("/a/b/c/d/", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.UiLocales)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndQueryModeParameters_IfTheQueryModeParameterIsAll_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&Key-3=value-3");

			Assert.AreEqual("/a/b/c/d/?Key-1=value-1&Key-2=value-2&key-3=value-3,value-3", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.All)).ToString());
		}

		[TestMethod]
		public async Task CreateRelativeAsync_WithSegmentsAndQueryModeParameters_IfTheQueryModeParameterIsUiLocales_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/a/b/c/d/", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.UiLocales)).ToString());

			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&key-3=value-3&Key-1=value-1&ReturnUrl=%2Fpath-1%2Fpath-2%2F%3Fui_locales%3Dsv%20fr%20en");
			Assert.AreEqual("/a/b/c/d/?UI_Locales=sv%20fr%20en", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.UiLocales)).ToString());

			uriFactory = await this.CreateUriFactoryAsync(null, "?Key-2=value-2&ui_LOCALES=fi&key-3=value-3&Key-1=value-1");
			Assert.AreEqual("/a/b/c/d/?UI_Locales=fi", (await uriFactory.CreateRelativeAsync(new[] { "a", "b", "c", "d" }, UriFactoryQueryMode.UiLocales)).ToString());
		}

		protected internal virtual async Task<UriFactory> CreateUriFactoryAsync(string path = null, string query = null)
		{
			PathString? pathString = null;

			if(path != null)
				pathString = new PathString(path);

			QueryString? queryString = null;

			if(query != null)
				queryString = new QueryString(query);

			var httpContext = await HttpContextFactory.CreateAsync(pathString, queryString);

			return await this.CreateUriFactoryAsync(httpContext);
		}

		protected internal virtual async Task<UriFactory> CreateUriFactoryAsync(HttpContext httpContext)
		{
			var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
			httpContextAccessorMock.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(httpContext);
			var httpContextAccessor = httpContextAccessorMock.Object;

			var logger = Mock.Of<ILogger>();

			var loggerFactoryMock = new Mock<ILoggerFactory>();
			loggerFactoryMock.Setup(loggerFactory => loggerFactory.CreateLogger(It.IsAny<string>())).Returns(logger);
			var loggerFactory = loggerFactoryMock.Object;

			return await Task.FromResult(new UriFactory(httpContextAccessor, loggerFactory));
		}

		[TestMethod]
		public async Task TrailingPathSlash_Set_ShouldWork()
		{
			var uriFactory = await this.CreateUriFactoryAsync();

			uriFactory.TrailingPathSlash = false;
			Assert.IsFalse(uriFactory.TrailingPathSlash);

			uriFactory.TrailingPathSlash = true;
			Assert.IsTrue(uriFactory.TrailingPathSlash);
		}

		[TestMethod]
		public async Task TrailingPathSlash_ShouldReturnTrueByDefault()
		{
			var uriFactory = await this.CreateUriFactoryAsync();
			Assert.IsTrue(uriFactory.TrailingPathSlash);
		}

		#endregion
	}
}