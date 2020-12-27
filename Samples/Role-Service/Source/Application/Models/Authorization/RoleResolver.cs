using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models.Authorization.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Models.Authorization
{
	public class RoleResolver : IRoleResolver
	{
		#region Constructors

		public RoleResolver(IEnumerable<IRoleLoader> loaders, IOptions<RoleResolvingOptions> options)
		{
			this.Loaders = loaders ?? throw new ArgumentNullException(nameof(loaders));
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
		}

		#endregion

		#region Properties

		public virtual IEnumerable<IRoleLoader> Loaders { get; }
		public virtual IOptions<RoleResolvingOptions> Options { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<string> GetClientIdAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var clientIdKey = this.Options.Value.ClientIdKey;

			if(clientIdKey != null && httpContext.User != null)
				return await Task.FromResult(httpContext.User.FindFirst(clientIdKey)?.Value);

			return null;
		}

		protected internal virtual async Task<IEnumerable<IRoleLoader>> GetLoadersAsync(string clientId)
		{
			// Now we return all registerd loaders. We could later use some kind of mapping, clientId => loader.
			return await Task.FromResult(this.Loaders);
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public virtual async Task<IEnumerable<string>> ResolveAsync(HttpContext httpContext)
		{
			if(httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var clientId = await this.GetClientIdAsync(httpContext);

			var exceptions = new List<Exception>();

			foreach(var loader in await this.GetLoadersAsync(clientId))
			{
				try
				{
					return await loader.ListAsync(httpContext.User);
				}
				catch(Exception exception)
				{
					exceptions.Add(exception);
				}
			}

			throw new AggregateException(exceptions);
		}

		#endregion
	}
}