using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Helpers
{
	public static class HttpContextFactory
	{
		#region Methods

		public static async Task<HttpContext> CreateAsync(PathString? path = null, QueryString? query = null)
		{
			var httpContext = new DefaultHttpContext();

			// This will add the IQueryFeature to the features collection.
			var _ = httpContext.Request.Query;

			if(path != null)
				httpContext.Request.Path = path.Value;

			if(query != null)
				httpContext.Request.QueryString = query.Value;

			return await Task.FromResult(httpContext);
		}

		#endregion
	}
}