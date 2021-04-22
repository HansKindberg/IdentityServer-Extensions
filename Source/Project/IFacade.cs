using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.Web.Authorization;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Decoration;
using Rsk.Saml.Services;

namespace HansKindberg.IdentityServer
{
	public interface IFacade
	{
		#region Properties

		IAuthenticationSchemeLoader AuthenticationSchemeLoader { get; }
		IAuthorizationResolver AuthorizationResolver { get; }
		IClientStore ClientStore { get; }
		IDecorationLoader DecorationLoader { get; }
		IEventService Events { get; }
		IOptionsMonitor<ExceptionHandlingOptions> ExceptionHandling { get; }
		IFeatureManager FeatureManager { get; }
		IHttpContextAccessor HttpContextAccessor { get; }
		IIdentityFacade Identity { get; }
		IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServer { get; }
		IIdentityServerInteractionService Interaction { get; }
		IStringLocalizerFactory LocalizerFactory { get; }
		ILoggerFactory LoggerFactory { get; }
		IOptionsMonitor<RequestLocalizationOptions> RequestLocalization { get; }

		/// <summary>
		/// The SAML-interaction-service if the SAML-feature is enabled otherwise null.
		/// </summary>
		ISamlInteractionService SamlInteraction { get; }

		#endregion
	}
}