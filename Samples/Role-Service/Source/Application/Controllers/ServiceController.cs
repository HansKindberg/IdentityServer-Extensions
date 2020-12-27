using System;
using HansKindberg.RoleService.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HansKindberg.RoleService.Controllers
{
	public abstract class ServiceController : ControllerBase
	{
		#region Constructors

		protected ServiceController(ILoggerFactory loggerFactory, ISettings settings)
		{
			if(loggerFactory == null)
				throw new ArgumentNullException(nameof(loggerFactory));

			this.Logger = loggerFactory.CreateLogger(this.GetType());
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		#endregion

		#region Properties

		protected internal virtual ILogger Logger { get; }
		protected internal virtual ISettings Settings { get; }

		#endregion
	}
}