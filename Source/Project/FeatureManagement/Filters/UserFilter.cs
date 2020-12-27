using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Extensions;
using HansKindberg.IdentityServer.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Extensions;
using RegionOrebroLan.Logging.Extensions;
using RegionOrebroLan.Web.Authentication.Security.Claims.Extensions;

namespace HansKindberg.IdentityServer.FeatureManagement.Filters
{
	public class UserFilter : IFeatureFilter
	{
		#region Constructors

		public UserFilter(IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
		{
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IHttpContextAccessor HttpContextAccessor { get; }
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		public virtual async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			var enabled = false;

			var httpContext = this.HttpContextAccessor.HttpContext;

			// ReSharper disable InvertIf
			if(httpContext != null && !httpContext.SignedOut())
			{
				var options = context.Parameters.Get<UserFilterOptions>();
				var user = httpContext.User;

				enabled = await this.EvaluateAsync(options, user);

				if(!enabled)
					this.Logger.LogDebugIfEnabled($"The {context.FeatureName}-feature is not enabled for user {user?.Identity?.Name.ToStringRepresentation()}.");
			}
			// ReSharper restore InvertIf

			return await Task.FromResult(enabled);
		}

		protected internal virtual async Task<bool> EvaluateAsync(UserFilterOptions options, ClaimsPrincipal user)
		{
			await Task.CompletedTask;

			if(options == null)
				return false;

			var names = options.Names.Where(name => !string.IsNullOrWhiteSpace(name)).ToHashSet(StringComparer.OrdinalIgnoreCase);

			if(!names.Any())
				return false;

			var providers = options.Providers.Where(provider => !string.IsNullOrWhiteSpace(provider)).ToHashSet(StringComparer.OrdinalIgnoreCase);

			if(!providers.Any())
				return false;

			// ReSharper disable UseNullPropagation
			if(user == null)
				return false;
			// ReSharper restore UseNullPropagation

			if(user.Identity == null)
				return false;

			if(!user.Identity.IsAuthenticated)
				return false;

			if(user.Identity.Name == null)
				return false;

			var identityProviderClaim = user.Claims.FindFirstIdentityProviderClaim()?.Value;

			if(string.IsNullOrWhiteSpace(identityProviderClaim))
				return false;

			return names.Any(name => user.Identity.Name.Like(name)) && providers.Any(provider => identityProviderClaim.Like(provider));
		}

		#endregion
	}
}