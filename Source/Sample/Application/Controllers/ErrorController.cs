using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Application.Models.Views.Error;
using HansKindberg.IdentityServer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RegionOrebroLan.Logging.Extensions;

namespace Application.Controllers
{
	public class ErrorController : SiteController
	{
		#region Constructors

		public ErrorController(IFacade facade) : base(facade) { }

		#endregion

		#region Methods

		public virtual async Task<IActionResult> Index(string errorId)
		{
			var model = new ErrorViewModel
			{
				Detailed = this.Facade.ExceptionHandling.CurrentValue.Detailed,
				IdentityServerError = await this.Facade.Interaction.GetErrorContextAsync(errorId)
			};

			if(model.IdentityServerError == null)
			{
				var exceptionHandlerPathFeature = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();

				model.ActivityId = Activity.Current?.Id;
				model.Exception = exceptionHandlerPathFeature.Error;
				model.Path = exceptionHandlerPathFeature.Path;
				model.TraceIdentifier = this.HttpContext.TraceIdentifier;

				var details = new List<string>
				{
					"Details:",
					$"activity-id = \"{Activity.Current?.Id}\"",
					$"path = \"{exceptionHandlerPathFeature.Path}\"",
					$"trace-identifier = \"{this.HttpContext.TraceIdentifier}\""
				};

				this.Logger.LogErrorIfEnabled(exceptionHandlerPathFeature.Error, string.Join(Environment.NewLine + " - ", details));
			}
			else
			{
				var error = model.IdentityServerError;

				var details = new List<string>
				{
					"Identity-Server error:",
					$"client-id = \"{error.ClientId}\"",
					$"display-mode = \"{error.DisplayMode}\"",
					$"error = \"{error.Error}\"",
					$"error-description = \"{error.ErrorDescription}\"",
					$"redirect-uri = \"{error.RedirectUri}\"",
					$"request-id = \"{error.RequestId}\"",
					$"response-mode = \"{error.ResponseMode}\"",
					$"ui-locales = \"{error.UiLocales}\""
				};

				this.Logger.LogErrorIfEnabled(string.Join(Environment.NewLine + " - ", details));
			}

			return this.View(model);
		}

		#endregion
	}
}