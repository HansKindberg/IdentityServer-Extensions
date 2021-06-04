using System;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.Web.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Web.Authentication;
using RegionOrebroLan.Web.Authentication.Decoration;
using Rsk.Saml.Services;

namespace HansKindberg.IdentityServer
{
	public class Facade : IFacade
	{
		#region Constructors

		public Facade(IAuthenticationSchemeLoader authenticationSchemeLoader, IAuthorizationResolver authorizationResolver, IClientStore clientStore, IDecorationLoader decorationLoader, IEventService events, IOptionsMonitor<ExceptionHandlingOptions> exceptionHandling, IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, IIdentityFacade identity, IOptionsMonitor<ExtendedIdentityServerOptions> identityServer, IIdentityServerInteractionService interaction, IStringLocalizerFactory localizerFactory, ILoggerFactory loggerFactory, IOptionsMonitor<RequestLocalizationOptions> requestLocalization, IServiceProvider serviceProvider)
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

			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			if(featureManager.IsEnabled(Feature.Saml))
				this.SamlInteraction = serviceProvider.GetRequiredService<ISamlInteractionService>();
		}

		#endregion

		#region Properties

		public virtual IAuthenticationSchemeLoader AuthenticationSchemeLoader { get; }
		public virtual IAuthorizationResolver AuthorizationResolver { get; }
		public virtual IClientStore ClientStore { get; }
		public virtual IDecorationLoader DecorationLoader { get; }
		public virtual IEventService Events { get; }
		public virtual IOptionsMonitor<ExceptionHandlingOptions> ExceptionHandling { get; }
		public virtual IFeatureManager FeatureManager { get; }
		public virtual IHttpContextAccessor HttpContextAccessor { get; }
		public virtual IIdentityFacade Identity { get; }
		public virtual IOptionsMonitor<ExtendedIdentityServerOptions> IdentityServer { get; }
		public virtual IIdentityServerInteractionService Interaction { get; }
		public virtual IStringLocalizerFactory LocalizerFactory { get; }
		public virtual ILoggerFactory LoggerFactory { get; }
		public virtual IOptionsMonitor<RequestLocalizationOptions> RequestLocalization { get; }
		public virtual ISamlInteractionService SamlInteraction { get; }

		#endregion
	}
}