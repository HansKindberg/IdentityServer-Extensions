using System.Linq;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Web.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Web.Mvc.Extensions
{
	[TestClass]
	public class ActionContextExtensionTest
	{
		#region Methods

		protected internal virtual ActionContext CreateActionContext()
		{
			return new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
		}

		[TestMethod]
		public async Task Test()
		{
			await Task.CompletedTask;

			var actionContext = this.CreateActionContext();

			Assert.IsFalse(actionContext.HttpContext.Response.Headers.Any());

			actionContext.EnsureResponseHeader("key", "value");

			Assert.AreEqual(1, actionContext.HttpContext.Response.Headers.Count);
			var (key, value) = actionContext.HttpContext.Response.Headers.First();
			Assert.AreEqual("key", key);
			Assert.AreEqual(1, value.Count);
			Assert.AreEqual("value", value.First());

			actionContext.EnsureResponseHeader("key", "another-value");

			Assert.AreEqual(1, actionContext.HttpContext.Response.Headers.Count);
			(key, value) = actionContext.HttpContext.Response.Headers.First();
			Assert.AreEqual("key", key);
			Assert.AreEqual(1, value.Count);
			Assert.AreEqual("value", value.First());
		}

		#endregion
	}
}