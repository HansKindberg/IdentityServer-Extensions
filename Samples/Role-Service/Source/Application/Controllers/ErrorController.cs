using System;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models;
using HansKindberg.RoleService.Models.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;

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

		[Route("Error")]
		public virtual async Task<IActionResult> Index()
		{
			var exception = this.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
			var options = this.OptionsMonitor.CurrentValue;

			if(exception != null)
				this.Logger.LogErrorIfEnabled(exception, "Error");

			return await Task.FromResult(
				this.Problem(
					options.Detailed ? exception?.ToString() : null,
					title: exception is ServiceException ? exception.Message : "Error"
				)
			);
		}

		#endregion
	}
}