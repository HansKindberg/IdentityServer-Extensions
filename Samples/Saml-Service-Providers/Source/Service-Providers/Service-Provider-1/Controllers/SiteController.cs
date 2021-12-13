using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Controllers
{
	[SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
	public abstract class SiteController : Controller
	{
		#region Constructors

		protected SiteController(ILoggerFactory loggerFactory)
		{
			this.Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		protected internal virtual async Task<string> ResolveAndValidateReturnUrlAsync(string returnUrl)
		{
			returnUrl = await this.ResolveReturnUrlAsync(returnUrl);

			if(this.Url.IsLocalUrl(returnUrl))
				return returnUrl;

			var message = $"The return-url \"{returnUrl}\" is invalid.";

			if(this.Logger.IsEnabled(LogLevel.Error))
				this.Logger.LogError(message);

			throw new InvalidOperationException(message);
		}

		protected internal virtual async Task<string> ResolveReturnUrlAsync(string returnUrl)
		{
			return await Task.FromResult(string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl);
		}

		#endregion
	}
}