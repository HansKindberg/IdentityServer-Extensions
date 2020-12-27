using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.Web.Mvc.Filters;
using HansKindberg.IdentityServer.Web.Mvc.Filters.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTests.Web.Mvc.Filters
{
	[TestClass]
	public class SecurityHeadersFilterTest
	{
		#region Methods

		protected internal virtual ResultExecutingContext CreateResultExecutingContext()
		{
			return this.CreateResultExecutingContext(Mock.Of<IActionResult>());
		}

		protected internal virtual ResultExecutingContext CreateResultExecutingContext(IActionResult result)
		{
			var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

			return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, new object());
		}

		protected internal virtual SecurityHeadersFilter CreateSecurityHeadersFilter(bool enabled)
		{
			var featureManagerMock = new Mock<IFeatureManager>();

			featureManagerMock.Setup(featureManager => featureManager.IsEnabledAsync(nameof(Feature.SecurityHeaders))).Returns(Task.FromResult(enabled));

			return new SecurityHeadersFilter(featureManagerMock.Object, Options.Create(new SecurityHeaderOptions()));
		}

		[TestMethod]
		public async Task OnResultExecuting_HttpsConditional_Test()
		{
			await Task.CompletedTask;

			var securityHeadersFilter = this.CreateSecurityHeadersFilter(true);

			var context = this.CreateResultExecutingContext(new ViewResult());
			Assert.IsFalse(context.HttpContext.Request.IsHttps);
			var headers = context.HttpContext.Response.Headers;
			securityHeadersFilter.OnResultExecuting(context);
			var expectedValue = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';";
			var value = headers["Content-Security-Policy"].First();
			Assert.AreEqual(expectedValue, value);
			value = headers["X-Content-Security-Policy"].First();
			Assert.AreEqual(expectedValue, value);

			context = this.CreateResultExecutingContext(new ViewResult());
			context.HttpContext.Request.IsHttps = true;
			Assert.IsTrue(context.HttpContext.Request.IsHttps);
			headers = context.HttpContext.Response.Headers;
			securityHeadersFilter.OnResultExecuting(context);
			expectedValue = "default-src 'self'; object-src 'none'; frame-ancestors 'none'; sandbox allow-forms allow-same-origin allow-scripts; base-uri 'self';upgrade-insecure-requests;";
			value = headers["Content-Security-Policy"].First();
			Assert.AreEqual(expectedValue, value);
			value = headers["X-Content-Security-Policy"].First();
			Assert.AreEqual(expectedValue, value);
		}

		[TestMethod]
		public async Task OnResultExecuting_IfTheResultIsAViewResult_ShouldAddHeaders()
		{
			await Task.CompletedTask;

			var securityHeadersFilter = this.CreateSecurityHeadersFilter(true);
			var context = this.CreateResultExecutingContext(new ViewResult());

			var headers = context.HttpContext.Response.Headers;

			Assert.IsFalse(headers.Any());

			securityHeadersFilter.OnResultExecuting(context);

			Assert.AreEqual(5, headers.Count);
			Assert.IsTrue(headers.ContainsKey("X-Content-Type-Options"));
			Assert.IsTrue(headers.ContainsKey("X-Frame-Options"));
			Assert.IsTrue(headers.ContainsKey("Content-Security-Policy"));
			Assert.IsTrue(headers.ContainsKey("X-Content-Security-Policy"));
			Assert.IsTrue(headers.ContainsKey("Referrer-Policy"));
		}

		[TestMethod]
		public async Task OnResultExecuting_IfTheResultIsNotAViewResult_ShouldNotAddHeaders()
		{
			await Task.CompletedTask;

			var securityHeadersFilter = this.CreateSecurityHeadersFilter(true);
			var context = this.CreateResultExecutingContext();

			var headers = context.HttpContext.Response.Headers;

			Assert.IsFalse(headers.Any());

			securityHeadersFilter.OnResultExecuting(context);

			Assert.IsFalse(headers.Any());
		}

		[TestMethod]
		public async Task OnResultExecuting_IfTheSecurityHeadersFeatureIsNotEnabled_ShouldNotAddHeaders()
		{
			await Task.CompletedTask;

			var securityHeadersFilter = this.CreateSecurityHeadersFilter(false);
			var context = this.CreateResultExecutingContext(new ViewResult());

			var headers = context.HttpContext.Response.Headers;

			Assert.IsFalse(headers.Any());

			securityHeadersFilter.OnResultExecuting(context);

			Assert.IsFalse(headers.Any());
		}

		#endregion
	}
}