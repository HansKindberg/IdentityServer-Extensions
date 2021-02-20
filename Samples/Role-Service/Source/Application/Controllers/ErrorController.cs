using System;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models;
using HansKindberg.RoleService.Models.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Controllers
{
	[ApiController]
	public class ErrorController : ServiceController
	{
		#region Constructors

		public ErrorController(ILoggerFactory loggerFactory, IOptionsMonitor<ExceptionHandlingOptions> optionsMonitor) : base(loggerFactory)
		{
			this.OptionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
		}

		#endregion

		#region Properties

		protected internal virtual IOptionsMonitor<ExceptionHandlingOptions> OptionsMonitor { get; }

		#endregion

		#region Methods

		protected internal virtual ProblemDetails CreateProblemDetails(Exception exception)
		{
			var details = this.OptionsMonitor.CurrentValue.Detailed ? exception?.ToString() : null;
			var title = exception is ServiceException ? exception.Message : null;

			return this.ProblemDetailsFactory.CreateProblemDetails(this.HttpContext, 500, title, "Error", details);
		}

		[Route("Error")]
		public virtual async Task<IActionResult> Index()
		{
			var exception = this.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

			if(this.Logger.IsEnabled(LogLevel.Error))
				this.Logger.LogError(exception, "Error");

			var problemDetails = this.CreateProblemDetails(exception);

			return await Task.FromResult(new ObjectResult(problemDetails)
			{
				StatusCode = problemDetails.Status
			});
		}

		#endregion
	}
}