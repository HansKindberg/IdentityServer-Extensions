using System;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using HansKindberg.IdentityServer.Configuration;
using HansKindberg.IdentityServer.FeatureManagement;
using HansKindberg.IdentityServer.FeatureManagement.Extensions;
using HansKindberg.IdentityServer.Identity;
using HansKindberg.IdentityServer.Saml.Services;
using HansKindberg.IdentityServer.Security.Claims;
using HansKindberg.IdentityServer.Web.Authentication;
using HansKindberg.Web.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using RegionOrebroLan.Web.Authentication.Configuration;
using RegionOrebroLan.Web.Authentication.Decoration;

namespace HansKindberg.IdentityServer
{
	public class Facade : IFacade
	{
		#region Constructors

		public Facade(IOptionsMonitor<ExtendedAuthenticationOptions> authentication, IAuthenticationSchemeRetriever authenticationSchemeRetriever, IAuthorizationResolver authorizationResolver, IClaimsSelectionContextAccessor claimsSelectionContextAccessor, IClientStore clientStore, IDecorationLoader decorationLoader, IEventService events, IOptionsMonitor<ExceptionHandlingOptions> exceptionHandling, IFeatureManager featureManager, IHttpContextAccessor httpContextAccessor, IIdentityFacade identity, IOptionsMonitor<ExtendedIdentityServerOptions> identityServer, IIdentityServerInteractionService interaction, IStringLocalizerFactory localizerFactory, ILoggerFactory loggerFactory, IMutualTlsService mutualTlsService, IOptionsMonitor<RequestLocalizationOptions> requestLocalization, IServiceProvider serviceProvider, IUriFactory uriFactory)
		{
			this.Authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
			this.AuthenticationSchemeRetriever = authenticationSchemeRetriever ?? throw new ArgumentNullException(nameof(authenticationSchemeRetriever));
			this.AuthorizationResolver = authorizationResolver ?? throw new ArgumentNullException(nameof(authorizationResolver));
			this.ClaimsSelectionContextAccessor = claimsSelectionContextAccessor ?? throw new ArgumentNullException(nameof(claimsSelectionContextAccessor));
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
			this.MutualTlsService = mutualTlsService ?? throw new ArgumentNullException(nameof(mutualTlsService));
			this.RequestLocalization = requestLocalization ?? throw new ArgumentNullException(nameof(requestLocalization));

			if(serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			if(featureManager.IsEnabled(Feature.Saml))
				this.SamlInteraction = serviceProvider.GetRequiredService<IExtendedSamlInteractionService>();

			this.UriFactory = uriFactory ?? throw new ArgumentNullException(nameof(uriFactory));
		}

		#endregion

		#region Properties

		public virtual IOptionsMonitor<ExtendedAuthenticationOptions> Authentication { get; }
		public virtual IAuthenticationSchemeRetriever AuthenticationSchemeRetriever { get; }
		public virtual IAuthorizationResolver AuthorizationResolver { get; }
		public virtual IClaimsSelectionContextAccessor ClaimsSelectionContextAccessor { get; }
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
		public virtual IMutualTlsService MutualTlsService { get; }
		public virtual IOptionsMonitor<RequestLocalizationOptions> RequestLocalization { get; }
		public virtual IExtendedSamlInteractionService SamlInteraction { get; }
		public virtual IUriFactory UriFactory { get; }

		#endregion
	}
}