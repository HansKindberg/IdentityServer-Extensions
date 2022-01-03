using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HansKindberg.IdentityServer.Application.Models.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.IdentityServer.Application.Controllers
{
	public abstract class SiteController : Controller
	{
		#region Constructors

		protected SiteController(IFacade facade)
		{
			this.Facade = facade ?? throw new ArgumentNullException(nameof(facade));
			this.Localizer = (facade.LocalizerFactory ?? throw new ArgumentException("The localizer-factory property can not be null.", nameof(facade))).Create(this.GetType());
			this.Logger = (facade.LoggerFactory ?? throw new ArgumentException("The logger-factory property can not be null.", nameof(facade))).CreateLogger(this.GetType());
		}

		#endregion

		#region Properties

		protected internal virtual IFacade Facade { get; }
		protected internal virtual IStringLocalizer Localizer { get; }
		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		protected internal virtual string GetLocalizedValue(string key, params string[] argumentKeys)
		{
			if(key == null)
				throw new ArgumentNullException(nameof(key));

			var arguments = new List<object>();

			// ReSharper disable LoopCanBeConvertedToQuery
			foreach(var argumentKey in argumentKeys ?? Array.Empty<string>())
			{
				var localizedArgument = this.Localizer.GetString(argumentKey);

				arguments.Add(localizedArgument.ResourceNotFound ? argumentKey : localizedArgument);
			}
			// ReSharper restore LoopCanBeConvertedToQuery

			return this.Localizer.GetString(key, arguments.ToArray());
		}

		protected internal virtual async Task<IActionResult> Redirect(string redirectUrl, byte secondsBeforeRedirect)
		{
			this.HttpContext.Response.Headers["Location"] = string.Empty;
			this.HttpContext.Response.StatusCode = 200;

			return await Task.FromResult(this.View("Redirect", new RedirectViewModel { RedirectUrl = redirectUrl, SecondsBeforeRedirect = secondsBeforeRedirect }));
		}

		protected internal virtual string ResolveAndValidateReturnUrl(string returnUrl)
		{
			returnUrl = this.ResolveReturnUrl(returnUrl);

			// ReSharper disable InvertIf
			if(!this.Url.IsLocalUrl(returnUrl) && !this.Facade.Interaction.IsValidReturnUrl(returnUrl))
			{
				var message = $"The return-url \"{returnUrl}\" is invalid";

				this.Logger.LogErrorIfEnabled(message);

				throw new InvalidOperationException(message);
			}
			// ReSharper restore InvertIf

			return returnUrl;
		}

		protected internal virtual string ResolveReturnUrl(string returnUrl)
		{
			return string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl;
		}

		#endregion
	}
}