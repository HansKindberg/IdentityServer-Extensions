using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Controllers
{
	[Authorize]
	[Route("identity")]
	public class IdentityController : ControllerBase
	{
		#region Constructors

		public IdentityController(ILoggerFactory loggerFactory)
		{
			if(loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.Logger = loggerFactory.CreateLogger(this.GetType().FullName);
		}

		#endregion

		#region Properties

		protected internal virtual ILogger Logger { get; }

		#endregion

		#region Methods

		[HttpGet]
		[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
		[SuppressMessage("Style", "IDE0050:Convert to tuple")]
		public virtual async Task<IActionResult> Get()
		{
			var claims = this.User.Claims.Select(claim => new {claim.Type, claim.Value});

			if(this.Logger.IsEnabled(LogLevel.Information))
				this.Logger.LogInformation("claims: {claims}", claims);

			return await Task.FromResult(new JsonResult(claims));
		}

		#endregion
	}
}