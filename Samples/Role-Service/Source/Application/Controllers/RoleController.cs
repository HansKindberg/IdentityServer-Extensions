using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models;
using HansKindberg.RoleService.Models.Authorization;
using HansKindberg.RoleService.Models.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Controllers
{
	[ApiController]
	[Authorize]
	public class RoleController : ServiceController
	{
		#region Constructors

		public RoleController(IOptionsMonitor<ExceptionHandlingOptions> exceptionHandlingOptionsMonitor, ILoggerFactory loggerFactory, IRoleResolver resolver) : base(loggerFactory)
		{
			this.ExceptionHandlingOptionsMonitor = exceptionHandlingOptionsMonitor ?? throw new ArgumentNullException(nameof(exceptionHandlingOptionsMonitor));
			this.Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
		}

		#endregion

		#region Properties

		protected internal virtual IOptionsMonitor<ExceptionHandlingOptions> ExceptionHandlingOptionsMonitor { get; }
		protected internal virtual IRoleResolver Resolver { get; }

		#endregion

		#region Methods

		[Route("List")]
		public virtual async Task<IEnumerable<string>> List()
		{
			try
			{
				//return await Task.FromResult(this.User.Claims.Select(claim => claim.Type + ": " + claim.Value));
				return await this.Resolver.ResolveAsync(this.HttpContext);
			}
			catch(Exception exception)
			{
				var message = "Could not get role-list.";

				if(this.Logger.IsEnabled(LogLevel.Error))
					this.Logger.LogError(exception, message);

				if(!this.ExceptionHandlingOptionsMonitor.CurrentValue.ThrowExceptions)
					return Enumerable.Empty<string>();

				if(exception is ServiceException)
					message += " " + exception.Message;

				throw new ServiceException(message, exception);
			}
		}

		#endregion
	}
}