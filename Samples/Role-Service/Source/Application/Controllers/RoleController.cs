using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HansKindberg.RoleService.Models;
using HansKindberg.RoleService.Models.Configuration;
using HansKindberg.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegionOrebroLan.Logging.Extensions;

namespace HansKindberg.RoleService.Controllers
{
	[ApiController]
	[Authorize]
	public class RoleController : ServiceController
	{
		#region Constructors

		public RoleController(IAuthorizationResolver authorizationResolver, IOptionsMonitor<ExceptionHandlingOptions> exceptionHandlingOptionsMonitor, ILoggerFactory loggerFactory) : base(loggerFactory)
		{
			this.AuthorizationResolver = authorizationResolver ?? throw new ArgumentNullException(nameof(authorizationResolver));
			this.ExceptionHandlingOptionsMonitor = exceptionHandlingOptionsMonitor ?? throw new ArgumentNullException(nameof(exceptionHandlingOptionsMonitor));
		}

		#endregion

		#region Properties

		protected internal virtual IAuthorizationResolver AuthorizationResolver { get; }
		protected internal virtual IOptionsMonitor<ExceptionHandlingOptions> ExceptionHandlingOptionsMonitor { get; }

		#endregion

		#region Methods

		[Route("List")]
		public virtual async Task<IEnumerable<string>> List()
		{
			try
			{
				var policy = await this.AuthorizationResolver.GetPolicyAsync(this.User);

				return policy.Roles.OrderBy(item => item, StringComparer.OrdinalIgnoreCase);
			}
			catch(Exception exception)
			{
				var message = "Could not get role-list.";

				this.Logger.LogErrorIfEnabled(exception, message);

				if(!this.ExceptionHandlingOptionsMonitor.CurrentValue.ThrowExceptions)
					return Enumerable.Empty<string>();

				if(exception is ServiceException)
					message += $" {exception.Message}";

				throw new ServiceException(message, exception);
			}
		}

		#endregion
	}
}