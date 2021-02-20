using System;
using System.Diagnostics.CodeAnalysis;
using HansKindberg.RoleService.Models.Authorization.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Models.Configuration
{
	[SuppressMessage("Naming", "CA1724:Type names should not match namespaces")]
	public class Settings : ISettings
	{
		#region Constructors

		public Settings(IOptions<ExceptionHandlingOptions> exceptionHandling, IOptions<HostFilteringOptions> hostFiltering, IOptions<JwtBearerOptions> jwtBearer, IOptions<LoggerFilterOptions> loggerFilter, IOptions<RoleResolvingOptions> roleResolving)
		{
			this.ExceptionHandling = exceptionHandling ?? throw new ArgumentNullException(nameof(exceptionHandling));
			this.HostFiltering = hostFiltering ?? throw new ArgumentNullException(nameof(hostFiltering));
			this.JwtBearer = jwtBearer ?? throw new ArgumentNullException(nameof(jwtBearer));
			this.LoggerFilter = loggerFilter ?? throw new ArgumentNullException(nameof(loggerFilter));
			this.RoleResolving = roleResolving ?? throw new ArgumentNullException(nameof(roleResolving));
		}

		#endregion

		#region Properties

		public virtual IOptions<ExceptionHandlingOptions> ExceptionHandling { get; }
		public virtual IOptions<HostFilteringOptions> HostFiltering { get; }
		public virtual IOptions<JwtBearerOptions> JwtBearer { get; }
		public virtual IOptions<LoggerFilterOptions> LoggerFilter { get; }
		public virtual IOptions<RoleResolvingOptions> RoleResolving { get; }

		#endregion
	}
}