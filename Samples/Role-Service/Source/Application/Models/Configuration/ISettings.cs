using HansKindberg.RoleService.Models.Authorization.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HansKindberg.RoleService.Models.Configuration
{
	public interface ISettings
	{
		#region Properties

		IOptions<ExceptionHandlingOptions> ExceptionHandling { get; }
		IOptions<HostFilteringOptions> HostFiltering { get; }
		IOptions<JwtBearerOptions> JwtBearer { get; }
		IOptions<LoggerFilterOptions> LoggerFilter { get; }
		IOptions<RoleResolvingOptions> RoleResolving { get; }

		#endregion
	}
}