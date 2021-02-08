using System;
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

namespace HansKindberg.IdentityServer
{
	public class Facade : IFacade
	{
		#region Constructors

		public Facade(IAuthenticationSchemeLoader authenticationSchemeLoader, IAuthorizationResolver authorizationResolver, IClientStore clientStore, IDecorationLoader decorationLoader, IEventService events, IOptions<ExceptionHandlingOptions> exceptionHandling, IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, IIdentityFacade identity, IOptions<ExtendedIdentityServerOptions> identityServer, IIdentityServerInteractionService interaction, IStringLocalizerFactory localizerFactory, ILoggerFactory loggerFactory, IOptions<RequestLocalizationOptions> requestLocalization)
		{
			this.AuthenticationSchemeLoader = authenticationSchemeLoader ?? throw new ArgumentNullException(nameof(authenticationSchemeLoader));
			this.AuthorizationResolver = authorizationResolver ?? throw new ArgumentNullException(nameof(authorizationResolver));
			this.ClientStore = clientStore ?? throw new ArgumentNullException(nameof(clientStore));
			this.DecorationLoader = decorationLoader ?? throw new ArgumentNullException(nameof(decorationLoader));
			this.Events = events ?? throw new ArgumentNullException(nameof(events));
			this.ExceptionHandling = exceptionHandling ?? throw new ArgumentNullException(nameof(exceptionHandling));
			this.FeatureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			this.HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			this.Identity = identity ?? throw new ArgumentNullException(nameof(identity));
			this.IdentityServer = identityServer ?? throw new ArgumentNullException(nameof(identityServer));
			this.Interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
			this.LocalizerFactory = localizerFactory ?? throw new ArgumentNullException(nameof(localizerFactory));
			this.LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			this.RequestLocalization = requestLocalization ?? throw new ArgumentNullException(nameof(requestLocalization));
		}

		#endregion

		#region Properties

		public virtual IAuthenticationSchemeLoader AuthenticationSchemeLoader { get; }
		public virtual IAuthorizationResolver AuthorizationResolver { get; }
		public virtual IClientStore ClientStore { get; }
		public virtual IDecorationLoader DecorationLoader { get; }
		public virtual IEventService Events { get; }
		public virtual IOptions<ExceptionHandlingOptions> ExceptionHandling { get; }
		public virtual IFeatureManager FeatureManager { get; }
		public virtual IHttpContextAccessor HttpContextAccessor { get; }
		public virtual IIdentityFacade Identity { get; }
		public virtual IOptions<ExtendedIdentityServerOptions> IdentityServer { get; }
		public virtual IIdentityServerInteractionService Interaction { get; }
		public virtual IStringLocalizerFactory LocalizerFactory { get; }
		public virtual ILoggerFactory LoggerFactory { get; }
		public virtual IOptions<RequestLocalizationOptions> RequestLocalization { get; }

		#endregion
	}
}